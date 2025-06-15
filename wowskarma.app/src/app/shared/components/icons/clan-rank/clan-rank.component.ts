import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { ClanRole } from 'src/app/services/api/models/clan-role';
import { NgbTooltip } from "@ng-bootstrap/ng-bootstrap";

@Component({
  selector: 'icon-clan-rank',
  template: `<i [className]="clanRankIconName" [ngbTooltip]="clanRankDisplayName" placement="bottom" [title]="clanRankDisplayName"></i>`,
  styleUrls: ['./clan-rank.component.scss'],
  imports: [
    NgbTooltip
  ],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ClanRankComponent {
  clanRank = input.required();

  get clanRankIconName() {
    switch (this.clanRank()) {
      case ClanRole.Commander:
        return 'clanranks-commander';
      case ClanRole.ExecutiveOfficer:
        return 'clanranks-executiveofficer';
      case ClanRole.Recruiter:
        return 'clanranks-recruiter';
      case ClanRole.CommissionedOfficer:
        return 'clanranks-commissionedofficer';
      case ClanRole.Officer:
        return 'clanranks-officer';
      case ClanRole.Private:
        return 'clanranks-private';
      default:
        return null;
    }
  }

  get clanRankDisplayName() {
    switch (this.clanRank()) {
      case ClanRole.Commander:
        return 'Commander';
      case ClanRole.ExecutiveOfficer:
        return 'Executive Officer';
      case ClanRole.Recruiter:
        return 'Recruiter';
      case ClanRole.CommissionedOfficer:
        return 'Commissioned Officer';
      case ClanRole.Officer:
        return 'Officer';
      case ClanRole.Private:
        return 'Private';
      default:
        return null;
    }
  }

  constructor() { }
}
