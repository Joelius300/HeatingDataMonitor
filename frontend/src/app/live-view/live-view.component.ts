import { Component, OnInit, ViewChild, ElementRef, AfterViewInit, OnDestroy, Inject } from '@angular/core';
import uPlot from 'uplot';
import { ChartBuilderService } from '../services/chart-builder.service';
import { RealTimeService } from '../services/real-time.service';
import { HeatingData } from '../model/heating-data';
import { Subscription } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { API_BASE_URL } from '../model/API_BASE_URL';

@Component({
  selector: 'app-live-view',
  templateUrl: './live-view.component.html',
  styleUrls: ['./live-view.component.scss']
})
export class LiveViewComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('chartContainer')
  private chartContainer: ElementRef<HTMLDivElement>;
  private chartOptions: uPlot.Options;
  private chartData: number[][];
  private chart: uPlot;
  private subscription: Subscription;

  private readonly MaxCount = 100;

  constructor(private chartBuilder: ChartBuilderService,
              private realTimeService: RealTimeService,
              private httpClient: HttpClient,
              @Inject(API_BASE_URL) private apiBaseUrl: string) { }

  ngOnInit(): void {
    const degreeScaleKey = 'degreeCelsius';
    const percentScaleKey = 'percent';
    this.chartOptions = this.chartBuilder.getTimeChartOptions(
      [
        this.chartBuilder.getAxis({
          scale: degreeScaleKey,
          values: (u, vals, space) => vals.map((v) => +v.toFixed(1) + '°'),
        }),
        this.chartBuilder.getAxis({
          scale: percentScaleKey,
          values: (u, vals, space) => vals.map((v) => +v.toFixed(1) + '%'),
          grid: {show: false},
          side: 1 // right
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
        }),
        this.chartBuilder.getCelsiusSeries({
          scale: degreeScaleKey,
          label: 'Abgas',
          stroke: 'white'
        }),
        this.chartBuilder.getPercentSeries({
          scale: percentScaleKey,
          label: 'CO2',
          stroke: 'black'
        })
      ]);

    this.chartData = [[], [], [], [], [], [], []];
    this.httpClient.get<HeatingData[]>(`${this.apiBaseUrl}api/HeatingDataHistory/CachedValues?count=${this.MaxCount}`)
                   .subscribe(values => {
                     for (const value of values) {
                       this.addChartData(value);
                     }
                     this.chart?.setData(this.chartData);
                   });

    this.subscription = this.realTimeService.currentDataChange.subscribe((newData: HeatingData) => {
      this.addChartData(newData);
      this.chart?.setData(this.chartData);
    });
  }

  ngAfterViewInit(): void {
    this.chart = this.chartBuilder.createChart(this.chartOptions, this.chartData, this.chartContainer.nativeElement);
  }

  private addChartData(data: HeatingData) {
    this.chartData[0].push(Date.parse(data.ReceivedTime) / 1000);
    this.chartData[1].push(data.Boiler_1);
    this.chartData[2].push(data.Kessel);
    this.chartData[3].push(data.Puffer_Oben);
    this.chartData[4].push(data.Puffer_Unten);
    this.chartData[5].push(data.Abgas);
    this.chartData[6].push(data.CO2_Ist);

    const length = this.chartData[0].length;
    if (length > this.MaxCount) {
      for (const array of this.chartData) {
        array.splice(0, length - this.MaxCount);
      }
    }
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }
}
