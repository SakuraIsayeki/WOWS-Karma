import { HttpClient } from "@angular/common/http";
import { ChangeDetectionStrategy, Component, inject, Input } from "@angular/core";
import { DomSanitizer } from "@angular/platform-browser";
import { ActivatedRoute } from "@angular/router";
import { mergeMap, Observable, merge, switchMap, shareReplay } from "rxjs";
import { map } from "rxjs/operators";
import { filterNotNull, InputObservable } from "../../rxjs-operators";

@Component({
    selector: "html-loader",
    template: "<div [innerHtml]=\"content$ | async\"></div>",
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HtmlLoaderComponent {
    @Input()
    @InputObservable()
    path!: string;
    path$!: Observable<string>

    private http: HttpClient = inject(HttpClient);
    private route: ActivatedRoute = inject(ActivatedRoute);
    private sanitizer: DomSanitizer = inject(DomSanitizer);

    // Get the HTML content from the server, at the path specified by the path$ input.
    content$ = merge(this.path$.pipe(filterNotNull()), this.route.data.pipe(map(d => d["path"]), filterNotNull())).pipe(
        switchMap(path => this.http.get(path, { responseType: "text" })),
        map(html => this.sanitizer.bypassSecurityTrustHtml(html)),
        shareReplay(1),
    );
}
