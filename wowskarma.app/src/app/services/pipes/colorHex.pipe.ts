import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'colorHex',
    standalone: true
})
export class ColorHexPipe implements PipeTransform {

  transform(value: number | undefined) {
    const result = value?.toString(16).substring(2, 8);
    return !result?.startsWith('#') ? `#${result}` : result;
  }

}
