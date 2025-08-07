import { Pipe, type PipeTransform, inject } from "@angular/core";
import { DomSanitizer, type SafeHtml } from "@angular/platform-browser";

@Pipe({
    name: "bypassHtml",
    standalone: true,
})
export class BypassHtmlPipe implements PipeTransform {
    private sanitizer = inject(DomSanitizer);

    transform(value: string): SafeHtml {
        return this.sanitizer.bypassSecurityTrustHtml(value);
    }
}
