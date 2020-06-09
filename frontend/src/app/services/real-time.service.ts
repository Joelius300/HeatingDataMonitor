import { HeatingData } from './../model/heating-data';
import { Injectable, Output, EventEmitter } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';

@Injectable({
  providedIn: 'root'
})
export class RealTimeService {
  private hubConnection: HubConnection;

  public currentData: HeatingData;
  @Output() currentDataChange = new EventEmitter<HeatingData>();

  public lastArchivedData: HeatingData;
  @Output() lastArchivedDataChange = new EventEmitter<HeatingData>();

  public async startConnection(): Promise<void> {
    if (this.hubConnection) {
      return;
    }

    this.hubConnection = new HubConnectionBuilder()
                            .withUrl('/realTimeFeed')
                            .build();

    this.hubConnection.on('OnDataPointReceived', (args) => {
      this.onDataReceived(args as HeatingData);
    });

    this.hubConnection.on('OnDataPointArchived', (args) => {
      this.onDataArchived(args as HeatingData);
    });

    await this.hubConnection.start();
    this.onDataReceived(await this.hubConnection.invoke('GetCurrentHeatingData'));
    // Currently not needed but works as expected
    // this.onDataArchived(await this.hubConnection.invoke('GetLastArchivedHeatingData'));
  }

  private onDataReceived(heatingData: HeatingData): void {
    this.currentData = heatingData;
    this.currentDataChange.emit(this.currentData);
  }

  private onDataArchived(heatingData: HeatingData): void {
    this.lastArchivedData = heatingData;
    this.lastArchivedDataChange.emit(this.lastArchivedData);
  }

  public async stopConnection(): Promise<void> {
    if (!this.hubConnection) {
      return;
    }

    await this.hubConnection.stop();
    this.hubConnection = null;
  }
}
