import { Component, OnInit } from "@angular/core";
import { Collapse } from "bootstrap";
import { AuthService } from "../../../services/auth.service";

@Component({
    selector: "app-navbar",
    templateUrl: "./navbar.component.html",
    styleUrls: ["./navbar.component.scss"],
})
export class NavbarComponent implements OnInit {

    constructor(public authService: AuthService) { }

    ngOnInit(): void {
    }

}
