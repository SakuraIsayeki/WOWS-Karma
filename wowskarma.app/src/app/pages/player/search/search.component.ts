import { ChangeDetectionStrategy, Component, OnInit } from "@angular/core";
import { FormControl, FormGroup } from "@angular/forms";
import { BehaviorSubject, catchError, debounceTime, distinctUntilChanged, of, switchMap, tap } from "rxjs";
import { map } from "rxjs/operators";
import { PlayerService } from "../../../services/api/services";
import { filterNotNull, switchMapCatchError, tapAny } from "../../../shared/rxjs-operators";

@Component({
  selector: "app-search",
  templateUrl: "./search.component.html",
  styleUrls: ["./search.component.scss"],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SearchComponent implements OnInit {
  search = new FormGroup({
    username: new FormControl(""),
  });

  loading$ = new BehaviorSubject<boolean>(false);

  // Gets an Observable to detect when the search form value changes.
  result$ = this.search.valueChanges.pipe(
    map(s => s.username), // Select the username field
    debounceTime(300),
    distinctUntilChanged(),
    filterNotNull(),
    tap(() => this.loading$.next(true)), // Set loading to true when the search form value changes.
    switchMapCatchError(query => this.playerService.apiPlayerSearchQueryGet$Json({ query })), // Get the search results
    tapAny(() => () => this.loading$.next(false)) // Sets loading to false when the search results are received, regardless of whether the search was successful or not.
  );


  constructor(
    public playerService: PlayerService,
  ) { }

  ngOnInit(): void {


  }

  /*
  onSubmit() {
    this.playerService.apiPlayerSearchQueryGet$Json({ query: this.search.value.username as string })
      .subscribe(value => this.results = value);
  }
  */
}
