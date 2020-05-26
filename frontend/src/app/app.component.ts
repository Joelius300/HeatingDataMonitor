import { RealTimeService } from './services/real-time.service';
import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';
import { Observable } from 'rxjs';
import { Breakpoints, BreakpointObserver } from '@angular/cdk/layout';
import { map, shareReplay } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit, OnDestroy {
  title = 'heating-data-monitor';

  isHandset$: Observable<boolean> = this.breakpointObserver.observe(Breakpoints.Handset)
    .pipe(
      map(result => result.matches),
      shareReplay()
    );

  constructor(private breakpointObserver: BreakpointObserver,
              private realTimeService: RealTimeService) { }

  @HostListener('window:unload')
  disconnect(): Promise<void> {
    return this.realTimeService.stopConnection();
  }

  ngOnInit(): Promise<void> {
    return this.realTimeService.startConnection();
  }

  ngOnDestroy(): Promise<void> {
    return this.disconnect();
  }
}
