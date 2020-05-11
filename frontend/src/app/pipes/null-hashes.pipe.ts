import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'nullHashes'
})
export class NullHashesPipe implements PipeTransform {

  transform(value: unknown, ...args: unknown[]): unknown {
    const count = args.length > 0 &&
                  Number.isInteger(args[0] as number) &&
                  args[0] as number > 0
                  ? args[0] as number : 5;

    if (value === null || value === undefined) {
      return '#'.repeat(count);
    }

    return value;
  }

}
