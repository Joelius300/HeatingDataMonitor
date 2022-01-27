import { BrowserModule } from '@angular/platform-browser';
import { NgModule, LOCALE_ID } from '@angular/core';
import localeDeCh from '@angular/common/locales/de-CH';
import { environment } from '../environments/environment';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { registerLocaleData } from '@angular/common';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatCardModule } from '@angular/material/card';
import { MatMenuModule } from '@angular/material/menu';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { LayoutModule } from '@angular/cdk/layout';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { FlexLayoutModule } from '@angular/flex-layout';
import { HistoryComponent } from './history/history.component';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { API_BASE_URL } from './model/API_BASE_URL';
import { LiveViewComponent } from './live-view/live-view.component';
registerLocaleData(localeDeCh);

@NgModule({
  declarations: [
    AppComponent,
    DashboardComponent,
    HistoryComponent,
    LiveViewComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    MatGridListModule,
    MatCardModule,
    MatMenuModule,
    MatIconModule,
    MatButtonModule,
    LayoutModule,
    MatToolbarModule,
    MatFormFieldModule,
    MatInputModule,
    FormsModule,
    MatSidenavModule,
    MatListModule,
    FlexLayoutModule,
    NgxDaterangepickerMd.forRoot(),
  ],
  providers: [
    { provide: LOCALE_ID, useValue: 'de-CH' },
    { provide: API_BASE_URL, useValue: environment.apiBaseUrl }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
