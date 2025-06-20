import { HttpClient } from "@angular/common/http";
import { ChangeDetectionStrategy, Component, computed, inject, input } from "@angular/core";
import { DomSanitizer } from "@angular/platform-browser";
import { ActivatedRoute } from "@angular/router";
import { firstValueFrom } from "rxjs";
import { AsyncPipe } from "@angular/common";

@Component({
    selector: "html-loader",
    template: '<div [innerHtml]="content() | async"></div>',
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: true,
    imports: [AsyncPipe],
})
export class HtmlLoaderComponent {
    // No requirement, as it can also be loaded from snapshot data.
    path = input<string>();

    private http: HttpClient = inject(HttpClient);
    private route: ActivatedRoute = inject(ActivatedRoute);
    private sanitizer: DomSanitizer = inject(DomSanitizer);

    // Get the HTML content from the server, at the path specified by the path$ input.
    content = computed(async () => {
        const path = this.path() || this.route.snapshot.data["path"];
        
        if (!path) {
            return null;
        }
        
        let html = await firstValueFrom(this.http.get(path, {responseType: "text"}));
        return this.sanitizer.bypassSecurityTrustHtml(html);
    });
}
