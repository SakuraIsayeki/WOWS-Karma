import { Pipe, type PipeTransform, inject } from "@angular/core";
import { DomSanitizer } from "@angular/platform-browser";

@Pipe({
    name: "safeStyle",
    standalone: true
})
export class SafeStylePipe implements PipeTransform {
  private sanitizer = inject(DomSanitizer);

  transform(value: any) {
    return this.sanitizer.bypassSecurityTrustStyle(value);
  }
}
