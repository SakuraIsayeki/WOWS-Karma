import { ChangeDetectionStrategy, Component, OnInit } from "@angular/core";
import { FormControl, FormGroup } from "@angular/forms";
import { BehaviorSubject, debounceTime, distinctUntilChanged, switchMap, tap } from "rxjs";
import { map } from "rxjs/operators";
import { PlayerService } from "../../../services/api/services";

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
    map(s => s.username),
    debounceTime(300),
    distinctUntilChanged(),
    tap(() => this.loading$.next(true)), // Set loading to true when the search form value changes.
    switchMap(username => this.playerService.apiPlayerSearchQueryGet$Json({ query: username! })), // Get the search results
    tap({
      next: () => this.loading$.next(false), // Set loading to false when the search results are received.
      error: () => this.loading$.next(false) // Set loading to false on error.
    })
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
