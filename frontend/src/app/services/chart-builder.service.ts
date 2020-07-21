import { Injectable } from '@angular/core';
import uPlot from 'uplot';
import localeDeCh from '@angular/common/locales/de-CH';
import { R3TargetBinder } from '@angular/compiler';

@Injectable({
  providedIn: 'root'
})
export class ChartBuilderService {
  private readonly deChDateNames: uPlot.DateNames = {
    MMM: localeDeCh[6][1],
    MMMM: localeDeCh[6][2],
    WWW: localeDeCh[3][3],
    WWWW: localeDeCh[3][2]
  };

  private readonly deChAxisValues = [
    [3600 * 24 * 365, '{YYYY}', 7, '{YYYY}',],
    [3600 * 24 * 28, '{MMM}', 7, '{MMM}\n{YYYY}',],
    [3600 * 24, '{DD}.{MM}', 7, '{DD}.{MM}\n{YYYY}',],
    [3600, '{HH}', 4, '{HH}\n{DD}.{MM}',],
    [60, '{HH}:{mm}', 4, '{HH}:{mm}\n{DD}.{MM}',],
    [1, '{mm}:{ss}', 2, '{HH}:{mm}:{ss}\n{DD}.{MM}',],
    [1e-3, '{mm}:{ss}.{fff}', 2, '{HH}:{mm}:{ss}\n{DD}.{MM}']
  ];

  private readonly deChDateFormatter = (tpl: string) => uPlot.fmtDate(tpl, this.deChDateNames);

  private get baseAxisConfig(): uPlot.Axis {
    return {
      stroke: 'white',
      grid: {
        stroke: '#373737dd',
        width: 1.5
      }
    };
  }

  private get baseTimeAxisConfig(): uPlot.Axis {
    return {
      values: this.deChAxisValues
    };
  }

  private get timeSeriesConfig(): uPlot.Series {
    return {
      label: 'Zeit',
      value: '{DD}.{MM}.{YYYY} {HH}:{mm}:{ss}'
    };
  }

  private get baseSeriesConfig(): uPlot.Series {
    return {
      width: 1 / devicePixelRatio,
    };
  }

  private get baseCeliusSeriesConfig(): uPlot.Series {
    return {
      scale: '° C',
      value: (u: uPlot, v: number) => (v == null ? '-' : v.toFixed(2) + ' °'),
    };
  }

  public getAxis(options: uPlot.Axis): uPlot.Axis {
    return Object.assign(this.baseAxisConfig, options);
  }

  public getSeries(options: uPlot.Series): uPlot.Series {
    return Object.assign(this.baseSeriesConfig, options);
  }

  public getCelsiusSeries(options: uPlot.Series): uPlot.Series {
    return Object.assign(this.baseCeliusSeriesConfig, this.getSeries(options));
  }

  public getTimeChartOptions(xAxes: uPlot.Axis[],
                             dataSeries: uPlot.Series[],
                             title?: string,
                             height: number = 0,
                             width: number = 0): uPlot.Options {
    xAxes.splice(0, 0, this.getAxis(this.baseTimeAxisConfig)); // add the y axis (time axis)
    dataSeries.splice(0, 0, this.timeSeriesConfig); // add the series for the y axis (time series)
    return {
      width,
      height,
      fmtDate: this.deChDateFormatter,
      series: dataSeries,
      axes: xAxes,
      title
    };
  }

  public createChart(options: uPlot.Options, data: uPlot.AlignedData, target: HTMLElement): uPlot {
    options.width = target.clientWidth;
    options.height = target.clientHeight;

    return new uPlot(options, data, target);
  }

  constructor() { }
}
