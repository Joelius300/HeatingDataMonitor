import { Component, OnInit } from '@angular/core';
import * as moment from 'moment';

@Component({
  selector: 'app-history',
  templateUrl: './history.component.html',
  styleUrls: ['./history.component.scss']
})
export class HistoryComponent implements OnInit {
  public selected: { chosenLabel?: string; startDate: moment.Moment; endDate: moment.Moment; };

  constructor() { }

  ngOnInit(): void {
    const now = moment();
    this.selected = {
      startDate: moment(now).subtract(10, 'hours'),
      endDate: now.add(5, 'minutes'),
    };
  }
}
