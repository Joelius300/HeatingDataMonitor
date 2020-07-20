import { RealTimeService } from './services/real-time.service';
import { Component, OnInit, OnDestroy, HostListener, Inject, LOCALE_ID } from '@angular/core';
import { Observable } from 'rxjs';
import { Breakpoints, BreakpointObserver } from '@angular/cdk/layout';
import { map, shareReplay } from 'rxjs/operators';
import * as moment from 'moment';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit, OnDestroy {
  isHandset$: Observable<boolean> = this.breakpointObserver.observe(Breakpoints.Handset)
    .pipe(
      map(result => result.matches),
      shareReplay()
    );

  constructor(private breakpointObserver: BreakpointObserver,
              private realTimeService: RealTimeService,
              @Inject(LOCALE_ID) private localeId: string) { }

  @HostListener('window:unload')
  disconnect(): Promise<void> {
    return this.realTimeService.stopConnection();
  }

  ngOnInit(): Promise<void> {
    moment.locale(this.localeId);

    return this.realTimeService.startConnection();
  }

  ngOnDestroy(): Promise<void> {
    return this.disconnect();
  }
}
