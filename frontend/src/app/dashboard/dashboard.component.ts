import { RealTimeService } from './../services/real-time.service';
import { Component, OnInit, OnDestroy } from '@angular/core';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit, OnDestroy {

  constructor(public realTimeService: RealTimeService) { }

  ngOnInit(): void {
    this.realTimeService.startConnection();
  }

  ngOnDestroy(): void {
    this.realTimeService.stopConnection();
  }

}
