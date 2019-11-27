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
import { PermissaoService } from '../../../../../core/seguranca/permissao.service';
import { AutenticacaoService } from '../../../../../core/autenticacao/autenticacao.service';
import { Permissao } from '../../../../../core/models/permissao.model';


@Component({
	// tslint:disable-next-line:component-selector
	selector: 'versattily-rotina-list',
	templateUrl: './rotina-list.component.html',
	changeDetection: ChangeDetectionStrategy.OnPush
})
export class RotinaListaComponent implements OnInit, AfterViewInit {

	nomeRotina : string =  "Rotinas";
	permissoes : Array<Permissao>;

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
		private servicoService: ServicoService,
		private permissaoService: PermissaoService,
		private auth:AutenticacaoService) {

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

		this.carregarPermissoes();

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

	//-------------------------------------------------------------------------------------------------
	// Método para carregar as permissões da página----------------------------------------------------
	//-------------------------------------------------------------------------------------------------
	carregarPermissoes(){
		this.permissaoService.getPermissoes(this.auth.idUsuario, this.nomeRotina).subscribe(permissao => {
			this.permissoes = permissao;
			console.log(this.permissoes);
		});
	}

	//-------------------------------------------------------------------------------------------------
	// Método para verificar a permissão sobre componente----------------------------------------------
	//-------------------------------------------------------------------------------------------------
	verificarPermissao(acao:string){
		console.log('ação: ' + acao);

		if(this.permissoes === undefined || this.permissoes === null || this.permissoes.length === 0)
		{
			return false;
		}

		var encontrou = this.permissoes.filter(filtro => filtro.TX_DSC === acao);

		console.log(encontrou);

		if(encontrou === undefined || encontrou === null || encontrou.length === 0)
		{
			console.log('não encontrou ' + acao);
			return false;
		}
		else
		{
			console.log('encontrou ' + acao);
			return true;
		}
	}
}
