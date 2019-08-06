import { Component, OnInit, ChangeDetectorRef, Inject, Input } from '@angular/core';
import { SciLogoutService } from '@speak/ng-sc/logout';
import { ItemCommanderService } from '../item-commander.service';
import { ActivatedRoute } from '@angular/router';
import { LOCAL_STORAGE, WebStorageService } from 'angular-webstorage-service';
import { FastViewService } from '../fast-view/fastview.service';

@Component({
  selector: 'sc-fast-view-page',
  templateUrl: './fast-view-page.component.html',
  styleUrls: ['./fast-view-page.component.scss']
})
export class FastViewPageComponent implements OnInit {

  @Input()  
  selectedDatabase: any;
  itemId: any;
  showOpenPageLink:boolean;
  constructor(@Inject(LOCAL_STORAGE) private storage: WebStorageService,
    private cdRef: ChangeDetectorRef,
    private route: ActivatedRoute,
    public logoutService: SciLogoutService,
    private itemCommanderService: ItemCommanderService,
    private fastviewService: FastViewService) {
    this.standardFields = ["Statistics", "Lifetime", "Security", "Help", "Appearance", "Insert Options", "Workflow", "Publishing", "Tasks", "Validation Rules", "Layout"];
    let itemId = this.route.snapshot.queryParamMap.get('itemid');

    if(itemId != null && itemId!=""){
      this.load(itemId);
      this.selectedDatabase = this.route.snapshot.queryParamMap.get('database');
      this.showOpenPageLink = false;
    }

    this.showStandardFields = this.storage.get('standardfields');    
    this.fastviewService.search.subscribe(value => {
      this.itemId = value;
      this.showOpenPageLink = true;
      this.load(value);
    });
  }
  
  objectKeys = Object.keys;
  responseData: any;
  showStandardFields: boolean;
  standardFields: Array<string>;
  languages: any;

  isNavigationShown: boolean;
  fastViewLoading: boolean;
  ngOnInit() {

  }

  load(itemId){
    this.fastViewLoading = true;
    this.itemCommanderService.fastView(itemId, this.selectedDatabase).subscribe({
      next: response => {
        this.responseData = response;
        this.fastViewLoading = false;
        this.cdRef.detectChanges();
      }, error: response =>{        
       this.fastViewLoading = false;
       this.responseData = null;
      }
    });
  }

  ngAfterViewChecked() {
  }
  GetSection(language: string) {
    var data = Object.keys(this.responseData.Data[language]);
    if (!this.showStandardFields) {
      this.standardFields.forEach(element => {
        var i = data.indexOf(element);
        if (i > -1) {
          data.splice(i, 1);
        }
      });
    }
    return data;
  }

  ShowSection(section: string) {
    if (!this.showStandardFields) {
      return !this.standardFields.includes(section);
    }
    return true;
  }

  GetFields(language: string, section: string) {
    return this.responseData.Data[language][section].map(function (it) { return it })
  }

  changeStandardFields(){
    this.storage.set('standardfields',this.showStandardFields);
  }
}
