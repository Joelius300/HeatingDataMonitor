import { Component, OnInit, Inject, ViewChild, ElementRef } from '@angular/core';
import * as moment from 'moment';
import uPlot from 'uplot';
import { HttpClient } from '@angular/common/http';
import { HeatingData } from '../model/heating-data';
import { API_BASE_URL } from '../model/API_BASE_URL';
import { ChartBuilderService } from '../services/chart-builder.service';

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
              private httpClient: HttpClient,
              private chartBuilder: ChartBuilderService) { }

  ngOnInit(): void {
    const now = moment();
    this.selected = {
      startDate: moment(now).subtract(10, 'hours'),
      endDate: now.add(5, 'minutes'),
    };

    const degreeScaleKey = 'degreeCelsius';
    this.chartOptions = this.chartBuilder.getTimeChartOptions(
      [
        this.chartBuilder.getAxis({
          scale: degreeScaleKey,
          values: (u, vals, space) => vals.map((v) => +v.toFixed(1) + '°'),
        })
      ],
      [
        this.chartBuilder.getCelsiusSeries({
          scale: degreeScaleKey,
          label: 'Boiler',
          stroke: 'red',
        }),
        this.chartBuilder.getCelsiusSeries({
          scale: degreeScaleKey,
          label: 'Kessel',
          stroke: 'blue',
        }),
        this.chartBuilder.getCelsiusSeries({
          scale: degreeScaleKey,
          label: 'Puffer Oben',
          stroke: 'green',
        }),
        this.chartBuilder.getCelsiusSeries({
          scale: degreeScaleKey,
          label: 'Puffer Unten',
          stroke: 'yellow',
        })
      ]);
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

                      this.currentChart?.destroy();
                      this.currentChart = this.chartBuilder.createChart(this.chartOptions, chartData, this.chartContainer.nativeElement);
                   });
  }
}
