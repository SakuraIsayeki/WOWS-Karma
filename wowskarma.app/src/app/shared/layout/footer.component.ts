import {Component, Inject, OnInit} from '@angular/core';
import {AppConfigService} from "../../services/app-config.service";

@Component({
  selector: 'app-footer',
  templateUrl: './footer.component.html',
  styleUrls: ['./footer.component.scss']
})
export class FooterComponent implements OnInit {

  constructor(
    @Inject(AppConfigService) public appConfig: AppConfigService
  ) { }

  ngOnInit(): void {
  }

}
