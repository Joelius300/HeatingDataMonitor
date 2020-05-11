import { HeatingData } from './../model/heating-data';
import { Injectable, Output, EventEmitter } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';

@Injectable({
  providedIn: 'root'
})
export class RealTimeService {
  private hubConnection: HubConnection;
  private _currentData: HeatingData;

  @Output() currentDataChange = new EventEmitter<HeatingData>();

  public get currentData(): HeatingData {
    return this._currentData;
  }

  startConnection(): Promise<void> {
    if (this.hubConnection) {
      return;
    }

    this.hubConnection = new HubConnectionBuilder()
                            .withUrl('http://localhost:5000/realTimeFeed')
                            .build();

    this.hubConnection.on('ReceiveHeatingData', (args) => {
      this._currentData = args as HeatingData;
      this.currentDataChange.emit(this.currentData);
    });

    return this.hubConnection.start();
  }

  stopConnection(): Promise<void> {
    if (!this.hubConnection) {
      return;
    }

    return this.hubConnection.stop();
  }
}
