import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'nullHashes'
})
export class NullHashesPipe implements PipeTransform {
  transform(value: any, count: number = 3): any {
    if (value == null) {
      return '#'.repeat(count);
    }

    return value;
  }
}
