import { ChangeDetectionStrategy, Component, OnInit, signal } from "@angular/core";
import { FormControl, FormGroup } from "@angular/forms";
import { BehaviorSubject, catchError, debounceTime, distinctUntilChanged, of, switchMap, tap } from "rxjs";
import { map } from "rxjs/operators";
import { PlayerService } from "../../../services/api/services";
import { filterNotNull, switchMapCatchError, tapAny } from "../../../shared/rxjs-operators";
import { toSignal } from "@angular/core/rxjs-interop";

@Component({
  selector: "app-search",
  templateUrl: "./search.component.html",
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SearchComponent {
  search = new FormGroup({
    username: new FormControl(""),
  });

  loading = signal(false);

  // Gets an Observable to detect when the search form value changes.
  result = toSignal(this.search.valueChanges.pipe(
    map(s => s.username), // Select the username field
    debounceTime(300),
    distinctUntilChanged(),
    filterNotNull(),
    tap(() => this.loading.set(true)), // Set loading to true when the search form value changes.
    switchMapCatchError(query => this.playerService.apiPlayerSearchQueryGet$Json({ query })), // Get the search results
    map(results => results === null ? [] : results),
    tapAny(() => this.loading.set(false)) // Sets loading to false when the search results are received, regardless of whether the search was successful or not.
  ));

  constructor(
    public playerService: PlayerService,
  ) { }
}
