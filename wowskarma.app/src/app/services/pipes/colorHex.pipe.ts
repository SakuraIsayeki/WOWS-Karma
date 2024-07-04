import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  standalone: true,
  name: 'colorHex'
})
export class ColorHexPipe implements PipeTransform {

  transform(value: number | undefined) {
    const result = value?.toString(16).substring(2, 8);
    return !result?.startsWith('#') ? `#${result}` : result;
  }

}
