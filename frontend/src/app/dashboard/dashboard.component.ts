import { RealTimeService } from './../services/real-time.service';
import { Component } from '@angular/core';
import { BetriebsPhaseHK } from './../model/betriebsphase-hk.enum';
import { HeatingData } from '../model/heating-data';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent {
  public get Betriebsphase_HK1(): string {
    if (!this.realTimeService?.currentData) {
      return undefined;
    }

    return BetriebsPhaseHK[this.realTimeService.currentData.Betriebsphase_HK1];
  }

  constructor(public realTimeService: RealTimeService) { }
}
