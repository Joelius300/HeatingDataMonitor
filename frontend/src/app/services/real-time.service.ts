import { HeatingData } from '../model/heating-data';
import { Injectable, Output, EventEmitter, Inject } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { API_BASE_URL } from '../model/API_BASE_URL';

@Injectable({
  providedIn: 'root'
})
export class RealTimeService {
  private hubConnection: HubConnection;

  public currentData: HeatingData;
  @Output() currentDataChange = new EventEmitter<HeatingData>();

  public lastArchivedData: HeatingData;
  @Output() lastArchivedDataChange = new EventEmitter<HeatingData>();

  constructor(@Inject(API_BASE_URL) private apiBaseUrl: string) { }

  public async startConnection(): Promise<void> {
    if (this.hubConnection) {
      return;
    }

    this.hubConnection = new HubConnectionBuilder()
                            .withUrl(`${this.apiBaseUrl}realTimeFeed`)
                            .build();

    this.hubConnection.on('OnDataPointReceived', (args) => {
      this.onDataReceived(args as HeatingData);
    });

    await this.hubConnection.start();
    this.onDataReceived(await this.hubConnection.invoke('GetCurrentHeatingData'));
  }

  private onDataReceived(heatingData: HeatingData): void {
    this.currentData = heatingData;
    this.currentDataChange.emit(this.currentData);
  }

  public async stopConnection(): Promise<void> {
    if (!this.hubConnection) {
      return;
    }

    await this.hubConnection.stop();
    this.hubConnection = null;
  }
}
