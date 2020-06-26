import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { DashboardComponent } from './dashboard/dashboard.component';
import { HistoryComponent } from './history/history.component';

const routes: Routes = [
  { path: 'dash', component: DashboardComponent },
  { path: 'history', component: HistoryComponent },
  { path: '**', redirectTo: '/dash' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
