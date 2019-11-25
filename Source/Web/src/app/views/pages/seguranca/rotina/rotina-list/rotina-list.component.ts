// Angular
import { Component, OnInit, ElementRef, ViewChild, ChangeDetectionStrategy, OnDestroy, Input, AfterViewInit } from '@angular/core';
import { RotinaService } from '../../../../../core/seguranca/rotina.service';
import { HttpParams } from '@angular/common/http';
import { RotinaDataSource } from '../../../../../core/seguranca/rotina.datasource';
import { MatPaginator, MatSort } from '@angular/material';
import { tap, debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { merge } from 'rxjs/internal/observable/merge';
import { fromEvent } from 'rxjs';
import { Router, ActivatedRoute } from '@angular/router';
import { ServicoService } from '../../../../../core/seguranca/servico.service';


@Component({
	// tslint:disable-next-line:component-selector
	selector: 'versattily-rotina-list',
	templateUrl: './rotina-list.component.html',
	changeDetection: ChangeDetectionStrategy.OnPush
})
export class RotinaListaComponent implements OnInit, AfterViewInit {

	dataSource: RotinaDataSource;
	displayedColumns = ["TX_NOME", "TX_NOME", "descricaoServico", "editar"];
	tamanho: number;

	salvouSucesso: boolean = false;
	excluidoSucesso: boolean = false;

	@ViewChild(MatPaginator, { static: false }) paginator: MatPaginator;
	@ViewChild(MatSort, { static: false }) sort: MatSort;
	@ViewChild('filtro', { static: false }) filtro: ElementRef;

	constructor(
		private rotinaService: RotinaService,
		private router: Router,
		private activatedRoute: ActivatedRoute,
		private servicoService: ServicoService) {

	}


	ngOnInit(): void {



		this.activatedRoute.params.subscribe(params => {
			this.salvouSucesso = params['sucesso'] && params['sucesso'] == 'true' ? true : false;
		});
		this.activatedRoute.params.subscribe(params => {
			this.excluidoSucesso = params['excluido'] && params['excluido'] == 'true' ? true : false;
		});


		this.tamanho = 20;
		this.dataSource = new RotinaDataSource(this.rotinaService,this.servicoService);
		this.dataSource.loadRotinas('', 1, 10, "TX_NOME", false);
	}

	ngAfterViewInit() {

		// server-side search
		fromEvent(this.filtro.nativeElement, 'keyup')
			.pipe(
				debounceTime(150),
				distinctUntilChanged(),
				tap(() => {
					this.paginator.pageIndex = 0;
					this.loadLessonsPage();
				})
			)
			.subscribe();

		// reset the paginator after sorting
		this.sort.sortChange.subscribe(() => this.paginator.pageIndex = 0);

		// on sort or paginate events, load a new page
		merge(this.sort.sortChange, this.paginator.page)
			.pipe(
				tap(() => this.loadLessonsPage())
			)
			.subscribe();
	}

	loadLessonsPage() {
		this.dataSource.loadRotinas(
			this.filtro.nativeElement.value,
			this.paginator.pageIndex + 1,
			this.paginator.pageSize,
			this.sort.active,
			this.sort.direction != 'asc');
	}

	adicionarRotina() {
		this.router.navigateByUrl('/seguranca/rotinas/cadastro');
	}

	visualizarRotina(cd_usr) {
		this.router.navigate(['/seguranca/rotinas/cadastro', { id: cd_usr }]);
	}
}
