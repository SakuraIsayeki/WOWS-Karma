import { HttpClient } from "@angular/common/http";
import { ChangeDetectionStrategy, Component, inject, input } from "@angular/core";
import { DomSanitizer } from "@angular/platform-browser";
import { ActivatedRoute } from "@angular/router";
import { firstValueFrom } from "rxjs";
import { derivedAsync } from "ngxtension/derived-async";

@Component({
    selector: "html-loader",
    template: '<div [innerHtml]="content()"></div>',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HtmlLoaderComponent {
    // No requirement, as it can also be loaded from snapshot data.
    public readonly path = input<string>();

    private readonly http: HttpClient = inject(HttpClient);
    private readonly route: ActivatedRoute = inject(ActivatedRoute);
    private readonly sanitizer: DomSanitizer = inject(DomSanitizer);

    // Get the HTML content from the server, at the path specified by the path$ input.
    content = derivedAsync(async () => {
        const path = this.path() || this.route.snapshot.data["path"];

        if (!path) {
            return null;
        }

        let html = await firstValueFrom(this.http.get(path, { responseType: "text" }));
        return this.sanitizer.bypassSecurityTrustHtml(html);
    }, { initialValue: null });
}
