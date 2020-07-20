import { Component, OnInit, Inject, ViewChild, ElementRef } from '@angular/core';
import * as moment from 'moment';
import uPlot from 'uplot';
import { HttpClient } from '@angular/common/http';
import { HeatingData } from '../model/heating-data';
import { API_BASE_URL } from '../model/API_BASE_URL';
import localeDeCh from '@angular/common/locales/de-CH';

@Component({
  selector: 'app-history',
  templateUrl: './history.component.html',
  styleUrls: ['./history.component.scss']
})
export class HistoryComponent implements OnInit {
  @ViewChild('chartContainer')
  private chartContainer: ElementRef<HTMLDivElement>;
  private currentChart: uPlot;
  private chartOptions: uPlot.Options;
  public selected: { chosenLabel?: string; startDate: moment.Moment; endDate: moment.Moment; };

  constructor(@Inject(API_BASE_URL) private apiBaseUrl: string,
              private httpClient: HttpClient) { }

  ngOnInit(): void {
    const now = moment();
    this.selected = {
      startDate: moment(now).subtract(10, 'hours'),
      endDate: now.add(5, 'minutes'),
    };

    const dateNames: uPlot.DateNames = {
      MMM: localeDeCh[6][1],
      MMMM: localeDeCh[6][2],
      WWW: localeDeCh[3][3],
      WWWW: localeDeCh[3][2]
    };

    const degreeScaleKey = '° C';
    const degreeFormatter = (u: uPlot, v: number) => (v == null ? '-' : v.toFixed(2) + ' °');
    const seriesWidth = 1 / devicePixelRatio;

    this.chartOptions = {
      title: undefined,
      width: 0,  // dynamically set
      height: 0, // dynamically set
      fmtDate: tpl => uPlot.fmtDate(tpl, dateNames),
      series: [
        {
          label: 'Zeit',
          value: '{DD}.{MM}.{YYYY} {HH}:{mm}:{ss}'
        },
        {
          label: 'Boiler',
          scale: degreeScaleKey,
          value: degreeFormatter,
          stroke: 'red',
          width: seriesWidth,
        },
        {
          label: 'Kessel',
          scale: degreeScaleKey,
          value: degreeFormatter,
          stroke: 'blue',
          width: seriesWidth,
        },
        {
          label: 'Puffer Oben',
          scale: degreeScaleKey,
          value: degreeFormatter,
          stroke: 'green',
          width: seriesWidth,
        },
        {
          label: 'Puffer Unten',
          scale: degreeScaleKey,
          value: degreeFormatter,
          stroke: 'yellow',
          width: seriesWidth,
        },
      ],
      axes: [
        {
          // You might need to modify the type file for this to compile but the code handles it correctly
          values: [
            [3600 * 24 * 365,    '{YYYY}',            7,   '{YYYY}',                       ],
            [3600 * 24 * 28,     '{MMM}',             7,   '{MMM}\n{YYYY}',                ],
            [3600 * 24,          '{DD}.{MM}',         7,   '{DD}.{MM}\n{YYYY}',            ],
            [3600,               '{HH}',              4,   '{HH}\n{DD}.{MM}',              ],
            [60,                 '{HH}:{mm}',         4,   '{HH}:{mm}\n{DD}.{MM}',         ],
            [1,                  '{mm}:{ss}',         2,   '{HH}:{mm}:{ss}\n{DD}.{MM}',    ],
            [1e-3,               '{mm}:{ss}.{fff}',   2,   '{HH}:{mm}:{ss}\n{DD}.{MM}'     ]
          ]
        },
        {
          scale: degreeScaleKey,
          values: (u, vals, space) => vals.map((v) => +v.toFixed(1) + '°'),
        },
      ],
    };
  }

  ngModelChange(e: {startDate: moment.Moment, endDate: moment.Moment}): void {
    this.httpClient.get<HeatingData[]>(`${this.apiBaseUrl}api/HeatingDataHistory/MainTemperatures?from=${e.startDate.toISOString()}&to=${e.endDate.toISOString()}`)
                   .subscribe(data => {
                      const chartData = [
                        data.map(v => Date.parse(v.ReceivedTime) / 1000),
                        data.map(v => v.Boiler_1),
                        data.map(v => v.Kessel),
                        data.map(v => v.Puffer_Oben),
                        data.map(v => v.Puffer_Unten)
                      ];

                      this.chartOptions.width = this.chartContainer.nativeElement.clientWidth;
                      this.chartOptions.height = this.chartContainer.nativeElement.clientHeight;

                      this.currentChart?.destroy();
                      this.currentChart = new uPlot(this.chartOptions, chartData, this.chartContainer.nativeElement);
                   });
  }
}
