import { Component, OnInit } from "@angular/core";
import { FormControl, FormGroup } from "@angular/forms";
import { BehaviorSubject, debounceTime, distinctUntilChanged, switchMap, tap } from "rxjs";
import { map } from "rxjs/operators";
import { PlayerService } from "../../../services/api/services";

@Component({
  selector: "app-search",
  templateUrl: "./search.component.html",
  styleUrls: ["./search.component.scss"],
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
    tap(() => this.loading$.next(true)),
    switchMap(username => this.playerService.apiPlayerSearchQueryGet$Json({ query: username! })),
    tap(() => this.loading$.next(false), () => this.loading$.next(false))
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
