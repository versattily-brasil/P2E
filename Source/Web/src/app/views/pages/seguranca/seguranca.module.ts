// Angular
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
// Fake API Angular-in-memory
import { HttpClientInMemoryWebApiModule } from 'angular-in-memory-web-api';
// Translate Module
import { TranslateModule } from '@ngx-translate/core';
// NGRX
import { StoreModule } from '@ngrx/store';
import { EffectsModule } from '@ngrx/effects';
// UI
import { PartialsModule } from '../../partials/partials.module';
// Core
import { FakeApiService } from '../../../core/_base/layout';
import { TextMaskModule } from 'angular2-text-mask';


// // Core => Services
// import {
// 	customersReducer,
// 	CustomerEffects,
// 	CustomersService,
// 	productsReducer,
// 	ProductEffects,
// 	ProductsService,
// 	productRemarksReducer,
// 	ProductRemarkEffects,
// 	ProductRemarksService,
// 	productSpecificationsReducer,
// 	ProductSpecificationEffects,
// 	ProductSpecificationsService
// } from '../../../core/e-commerce';

// Core => Utils
import {
	HttpUtilsService,
	TypesUtilsService,
	LayoutUtilsService
} from '../../../core/_base/crud';

// Shared
import {
	ActionNotificationComponent,
	DeleteEntityDialogComponent,
	FetchEntityDialogComponent,
	UpdateStatusDialogComponent
} from '../../partials/content/crud';

// // Components
import { SegurancaComponent } from './seguranca.component';
import { UsuarioListaComponent } from './usuario/usuario-lista/usuario-lista.component';
import { UsuarioFormComponent } from './usuario/usuario-form/usuario-form.component';


// // Customers
// import { CustomersListComponent } from './customers/customers-list/customers-list.component';
// import { CustomerEditDialogComponent } from './customers/customer-edit/customer-edit.dialog.component';
// // Products
// import { ProductsListComponent } from './products/products-list/products-list.component';
// import { ProductEditComponent } from './products/product-edit/product-edit.component';
// import { RemarksListComponent } from './products/_subs/remarks/remarks-list/remarks-list.component';
// import { SpecificationsListComponent } from './products/_subs/specifications/specifications-list/specifications-list.component';
// import { SpecificationEditDialogComponent } from './products/_subs/specifications/specification-edit/specification-edit-dialog.component';
// // Orders
// import { OrdersListComponent } from './orders/orders-list/orders-list.component';
// import { OrderEditComponent } from './orders/order-edit/order-edit.component';

// Material
import {
	MatInputModule,
	MatPaginatorModule,
	MatProgressSpinnerModule,
	MatSortModule,
	MatTableModule,
	MatSelectModule,
	MatMenuModule,
	MatProgressBarModule,
	MatButtonModule,
	MatCheckboxModule,
	MatDialogModule,
	MatTabsModule,
	MatNativeDateModule,
	MatCardModule,
	MatRadioModule,
	MatIconModule,
	MatDatepickerModule,
	MatAutocompleteModule,
	MAT_DIALOG_DEFAULT_OPTIONS,
	MatSnackBarModule,
	MatTooltipModule
} from '@angular/material';
import { environment } from '../../../../environments/environment';
import { CoreModule } from '../../../core/core.module';
import { NgbProgressbarModule, NgbProgressbarConfig, NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { NgxPermissionsModule } from 'ngx-permissions';
import { GrupoListaComponent } from './grupo/grupo-lista/grupo-lista.component';
import { GrupoFormComponent } from './grupo/grupo-form/grupo-form.component';
import { RotinaListaComponent } from './rotina/rotina-list/rotina-list.component';
import { RotinaFormComponent } from './rotina/rotina-form/rotina-form.compoenent';
import { ParceiroNegocioListaComponent } from './parceironegocio/parceironegocio-lista/parceironegocio-lista.component';
import { ParceiroNegocioFormComponent } from './parceironegocio/parceironegocio-form/parceironegocio-form.component';
import { ServicoListaComponent } from './servico/servico-lista/servico-lista.component';
import { UsuarioComponent } from './usuario/usuario.componente';
import { RotinaComponent } from './rotina/rotina.component';
import { GrupoComponent } from './grupo/grupo.component';
import { ParceiroNegocioComponent } from './parceironegocio/parceironegocio.component';

// tslint:disable-next-line:class-name
const routes: Routes = [
	{
		path: '',
		component: SegurancaComponent,
		// canActivate: [ModuleGuard],
		// data: { moduleName: 'seguranca' },
		children: [
			{
				path: 'usuarios',
				component: UsuarioListaComponent
			},
			{
				path: 'usuarios/cadastro',
				component: UsuarioFormComponent,
			},
			{
				path: 'grupos',
				component: GrupoListaComponent
			},
			{
				path: 'grupos/cadastro',
				component: GrupoFormComponent,
			},
			{
				path: 'rotinas',
				component: RotinaListaComponent,
			},
			{
				path: 'rotinas/cadastro',
				component: RotinaFormComponent,
			},
			{
				path: 'parceironegocio',
				component: ParceiroNegocioListaComponent,
			},
			{
				path: 'parceironegocio/cadastro',
				component: ParceiroNegocioFormComponent
			},
			{
				path: 'servicos',
				component: ServicoListaComponent
			}
		]
	}
];

@NgModule({
	imports: [
		NgbModule,
		MatDialogModule,
		CommonModule,
		HttpClientModule,
		PartialsModule,
		NgxPermissionsModule.forChild(),
		RouterModule.forChild(routes),
		FormsModule,
		ReactiveFormsModule,
		TranslateModule.forChild(),
		MatButtonModule,
		MatMenuModule,
		MatSelectModule,
		MatInputModule,
		MatTableModule,
		MatAutocompleteModule,
		MatRadioModule,
		MatIconModule,
		MatNativeDateModule,
		MatProgressBarModule,
		MatDatepickerModule,
		MatCardModule,
		MatPaginatorModule,
		MatSortModule,
		MatCheckboxModule,
		MatProgressSpinnerModule,
		MatSnackBarModule,
		MatTabsModule,
		MatTooltipModule,
		NgbProgressbarModule,
		TextMaskModule,
		environment.isMockEnabled ? HttpClientInMemoryWebApiModule.forFeature(FakeApiService, {
			passThruUnknownUrl: true,
			dataEncapsulation: false
		}) : [],
		// StoreModule.forFeature('products', productsReducer),
		// EffectsModule.forFeature([ProductEffects]),
		// StoreModule.forFeature('customers', customersReducer),
		// EffectsModule.forFeature([CustomerEffects]),
		// StoreModule.forFeature('productRemarks', productRemarksReducer),
		// EffectsModule.forFeature([ProductRemarkEffects]),
		// StoreModule.forFeature('productSpecifications', productSpecificationsReducer),
		// EffectsModule.forFeature([ProductSpecificationEffects]),
	],
	providers: [
		
		{
			provide: MAT_DIALOG_DEFAULT_OPTIONS,
			useValue: {
				hasBackdrop: true,
				panelClass: 'kt-mat-dialog-container__wrapper',
				height: 'auto',
				width: '900px'
			}
		},
		TypesUtilsService,
		LayoutUtilsService,
		HttpUtilsService,
		// CustomersService,
		// ProductRemarksService,
		// ProductSpecificationsService,
		// ProductsService,
		TypesUtilsService,
		LayoutUtilsService
	],
	entryComponents: [
		ActionNotificationComponent,
		// CustomerEditDialogComponent,
		DeleteEntityDialogComponent,
		FetchEntityDialogComponent,
		UpdateStatusDialogComponent,
		// SpecificationEditDialogComponent
	],
	declarations: [

		SegurancaComponent,
		UsuarioListaComponent,
		UsuarioFormComponent,
		GrupoListaComponent,
		GrupoFormComponent,
		RotinaListaComponent,
		RotinaFormComponent,
		ParceiroNegocioListaComponent,
		ParceiroNegocioFormComponent,
		ServicoListaComponent,
		UsuarioComponent,
		RotinaComponent,
		GrupoComponent,
		ParceiroNegocioComponent


		// ECommerceComponent,
		// // Customers
		// CustomersListComponent,
		// CustomerEditDialogComponent,
		// // Orders
		// OrdersListComponent,
		// OrderEditComponent,
		// // Products
		// ProductsListComponent,
		// ProductEditComponent,
		// RemarksListComponent,
		// SpecificationsListComponent,
		// SpecificationEditDialogComponent,
	],
	exports: [
		UsuarioListaComponent,
		UsuarioFormComponent,
		GrupoListaComponent,
		GrupoFormComponent,
		RotinaListaComponent,
		RotinaFormComponent,
		ParceiroNegocioListaComponent,
		ParceiroNegocioFormComponent,
		ServicoListaComponent,
		UsuarioComponent,
		RotinaComponent,
		GrupoComponent,
		ParceiroNegocioComponent
	]
})
export class SegurancaModule { }
