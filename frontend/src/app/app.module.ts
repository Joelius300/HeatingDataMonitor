import { BrowserModule } from '@angular/platform-browser';
import { NgModule, LOCALE_ID } from '@angular/core';
import localeDeCh from '@angular/common/locales/de-CH';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { NullHashesPipe } from './pipes/null-hashes.pipe';
import { registerLocaleData } from '@angular/common';
registerLocaleData(localeDeCh);

@NgModule({
  declarations: [
    AppComponent,
    DashboardComponent,
    NullHashesPipe
  ],
  imports: [
    BrowserModule,
    AppRoutingModule
  ],
  providers: [{provide: LOCALE_ID, useValue: 'de-CH'}],
  bootstrap: [AppComponent]
})
export class AppModule { }
