// Angular
import { Component, OnInit, ElementRef, ViewChild, TemplateRef, ChangeDetectionStrategy, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { NgbModal, NgbModalOptions } from '@ng-bootstrap/ng-bootstrap';
import { Router, ActivatedRoute } from '@angular/router';
import { BehaviorSubject, Observable, Subscription } from 'rxjs';
import { Usuario, UsuarioStatus } from '../../../../../core/models/usuario.model';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { UsuarioService } from '../../../../../core/seguranca/usuario.service';
import { share, tap } from 'rxjs/operators';
import { Modulo } from '../../../../../core/models/modulo.model';
import { Grupo } from '../../../../../core/models/grupo.model';
import { OperacaoService } from '../../../../../core/seguranca/operacao.service';
import { RotinaService } from '../../../../../core/seguranca/rotina.service';
import { ServicoService } from '../../../../../core/seguranca/servico.service';
import { Servico } from '../../../../../core/models/servico.model';
import { Rotina } from '../../../../../core/models/rotina.model';
import { Operacao } from '../../../../../core/models/operacao.model';
import { RotinaOperacoes } from '../../../../../core/models/rotina-operacoes.model';
import { UsuarioGrupo } from '../../../../../core/models/usuario-grupo.model';
import { UsuarioModulo } from '../../../../../core/models/usuario-modulo.model';
import { RotinaUsuarioOperacao } from '../../../../../core/models/rotina-usuario-operacao.model';
import { PasswordValidation } from './password-validation';
import { PermissaoService } from '../../../../../core/seguranca/permissao.service';
import { AutenticacaoService } from '../../../../../core/autenticacao/autenticacao.service';
import { Permissao } from '../../../../../core/models/permissao.model';
import { ToastrService } from 'ngx-toastr';
@Component({
	// tslint:disable-next-line:component-selector
	selector: 'versattily-usuario-form',
	templateUrl: './usuario-form.component.html'
})



export class UsuarioFormComponent implements OnInit {

	titulo: string = "Visualizar Usuário";

	nomeRotina : string =  "Usuário";
	permissoes : Array<Permissao>;

	public mascara = ['(', /[1-9]/, /\d/, ')', ' ', /\d/, /\d/, /\d/, /\d/, '-', /\d/, /\d/, /\d/, /\d/];
	myModel :string;
	modoEdicao: boolean = false;

	listaStatus: UsuarioStatus[] = [{ OP_STATUS: '1', TX_DESC: "Ativo" }, { OP_STATUS: '0', TX_DESC: "Inativo" }];

	usuarioForm: FormGroup;
	hasFormErrors: boolean = false;

	loadingSalvar: boolean = false;

	usuario: Observable<Usuario>;
	listaModulos: Modulo[] = [];
	listaGrupos: Grupo[] = [];
	listaServicos: Servico[] = [];
	listaRotinas: Rotina[] = [];
	listaOperacoes: Operacao[] = [];


	listaModulosSelecionados: Modulo[] = [];
	listaGruposSelecionados: Grupo[] = [];
	listaRotinaOperacoesSelecionadas: RotinaOperacoes[] = [];
	cdSrvSelecionado = 0;
	cdRotSelecionada = 0;

	@ViewChild('content8', { static: true }) private modalSalvando: TemplateRef<any>;
	@ViewChild('content12', { static: true }) private modalExcluindo: TemplateRef<any>;
	@ViewChild('login', { static: false })
	public login:any;

	constructor(
		private modalService: NgbModal,
		private router: Router,
		private activatedRoute: ActivatedRoute,
		private usuarioFB: FormBuilder,
		private usuarioService: UsuarioService,
		private operacaoService: OperacaoService,
		private rotinaService: RotinaService,
		private servicoService: ServicoService,
		private permissaoService: PermissaoService,
		private auth:AutenticacaoService,
		private cd: ChangeDetectorRef,
		private toast:ToastrService
	) { }

	ngOnInit() {

		let f = this.usuarioForm = this.usuarioFB.group({
			CD_USR: [''],
			TX_NOME: ['', Validators.required],
			TX_LOGIN: ['', Validators.required],
			TX_SENHA: ['', Validators.required],
			CONFIRMA_SENHA: ['', Validators.required],
			OP_STATUS: [null, Validators.required],
			TX_EMAIL: ['',[Validators.email,Validators.required]],
			TX_TELEFONE: ['']
		}
		, {
			validator: PasswordValidation.MatchPassword // your validation method
		  }
		);


		let id = this.usuarioService.cdUserVisualizar;

		if (id == 0) {
			this.modoEdicao = true;
			this.titulo = "Cadastrar Usuário";
		}

		this.usuario = this.usuarioService.getUsuario(id).pipe(
			tap(usuario => {

				usuario.CONFIRMA_SENHA = usuario.TX_SENHA;

				this.usuarioForm.patchValue(usuario);
				

				const toSelect = this.listaStatus.find(c => c.OP_STATUS == usuario.OP_STATUS);
				f.get('OP_STATUS').setValue(toSelect);

				this.rotinaService.getRotinas().subscribe(rotinas => {
					this.listaRotinas = rotinas;

					this.operacaoService.getOperacoes().subscribe(operacoes => {
						this.listaOperacoes = operacoes;
						this.montarTabelas(usuario);
						this.cd.markForCheck();
					});

				});


			})
		);

		this.servicoService.getServicos().subscribe(servicos => {
			this.listaServicos = servicos;
		});


		this.carregarPermissoes();
	}
	getErrorMessage() {
		return this.usuarioForm.controls['TX_EMAIL'].hasError('required') ? 'Email precisa ser preenchido' :
			this.usuarioForm.controls['TX_EMAIL'].hasError('email') ? 'Não é um email válido' :
				'';
	}

	getErrorMessageLogin(error) {
		let message = ''
        if (this.usuarioForm.controls['TX_LOGIN'].hasError('required') ){
            message = 'Login precisa ser preenchido';
        }
        if (this.usuarioForm.controls['TX_LOGIN'].hasError('login')){
            message = error;
        } 
        if (this.usuarioForm.controls['TX_LOGIN'].hasError('notUnique')){
            message = 'Login já existente'            
        } 
        return message
	}

	getTitle() {
		return "Visualizar Usuário"
	}

	exibirModal(content) {
		this.modalService.open(content);
	}

	exibirModalSalvar(content) {

		this.hasFormErrors = false;
		const controls = this.usuarioForm.controls;

		// this.validarLogin();
		this.usuarioService.getValidarLogin(controls['TX_LOGIN'].value,controls['CD_USR'].value).subscribe(
			(res) => {		
			},
			(err) => {
				controls.TX_LOGIN.setErrors({notUnique: true });
				controls.TX_LOGIN.markAsTouched();
				 this.modalService.dismissAll();
			   },
			() => {
				/** check form */
				if (this.usuarioForm.invalid) {
					Object.keys(controls).forEach(controlName =>
						controls[controlName].markAsTouched()
					);
					this.toast.error("Realize as correções no formulário e tente novamente.", 'Não é possível salvar o usuário');
					this.hasFormErrors = true;
					return;
				}else{
					this.modalService.open(content);
				}

			}
		);

		
	}

	cancelar() {
		//this.router.navigateByUrl('/seguranca/usuarios');
		this.modalService.dismissAll();
		this.usuarioService.telaLista = true;
	}


	onSumbit(withBack: boolean = false) {
		this.hasFormErrors = false;
		const controls = this.usuarioForm.controls;

		/** check form */
		if (this.usuarioForm.invalid) {
			Object.keys(controls).forEach(controlName =>
				controls[controlName].markAsTouched()
			);

			this.hasFormErrors = true;
			return;
		}

		return;
	}

	salvar() {

		
		this.modalService.dismissAll();


		let ngbModalOptions: NgbModalOptions = {
			backdrop: 'static',
			keyboard: false
		};
		this.modalService.open(this.modalSalvando, ngbModalOptions);

		let usuarioSalvar = this.usuarioForm.value;
		usuarioSalvar.UsuarioGrupo = [];
		usuarioSalvar.UsuarioModulo = [];
		usuarioSalvar.RotinaUsuarioOperacao = [];

		this.listaGruposSelecionados.forEach(function (grupo) {

			let usuarioGrupoSalvar: UsuarioGrupo = new UsuarioGrupo();
			usuarioGrupoSalvar.CD_GRP = grupo.CD_GRP;
			usuarioGrupoSalvar.CD_USR = usuarioSalvar.CD_USR;
			usuarioSalvar.UsuarioGrupo.push(usuarioGrupoSalvar);
		});

		this.listaModulosSelecionados.forEach(function (modulo) {

			let usuarioModuloSalvar: UsuarioModulo = new UsuarioModulo();
			usuarioModuloSalvar.CD_MOD = modulo.CD_MOD;
			usuarioModuloSalvar.CD_USR = usuarioSalvar.CD_USR;
			usuarioSalvar.UsuarioModulo.push(usuarioModuloSalvar);
		});

		this.listaRotinaOperacoesSelecionadas.forEach(function (rotinaOp) {

			rotinaOp.operacoes.forEach(function (op) {

				if (op.selecionada) {

					let rotinaUsuarioOperacaoSalvar: RotinaUsuarioOperacao = new RotinaUsuarioOperacao();
					rotinaUsuarioOperacaoSalvar.CD_USR = usuarioSalvar.CD_USR;
					rotinaUsuarioOperacaoSalvar.CD_OPR = op.CD_OPR;
					rotinaUsuarioOperacaoSalvar.CD_ROT = rotinaOp.rotina.CD_ROT;

					usuarioSalvar.RotinaUsuarioOperacao.push(rotinaUsuarioOperacaoSalvar);
				}
			});
		});

		
		let statusSalvar = this.listaStatus.find(o=>o.OP_STATUS == usuarioSalvar.OP_STATUS.OP_STATUS).OP_STATUS;

		usuarioSalvar.OP_STATUS = statusSalvar;

		this.usuarioService.salvarUsuario(usuarioSalvar).subscribe(
			result => {

			this.modalService.dismissAll();
			this.usuarioService.telaLista = true;
			// this.usuarioService.sucessoSalvar = true;
			this.toast.success("Registro salvo com sucesso!", 'Notificação');
			this.cd.markForCheck();
			// this.router.navigate(['/seguranca/usuarios', { sucesso: "true" }]);			
			},
			error => {
				this.modalService.dismissAll();
			   }
		);

	}

	montarTabelas(u) {

		this.listaModulosSelecionados = [];
		this.listaModulos = [];
		this.listaGruposSelecionados = [];
		this.listaGrupos = [];

		let comp = this;

		u.Grupo.forEach(function (item) {
			if (u.UsuarioGrupo.filter(e => e.CD_GRP == item.CD_GRP).length > 0) {
				comp.listaGruposSelecionados.push(item);
			} else {
				comp.listaGrupos.push(item);
			}
		});

		u.Modulo.forEach(function (item) {
			if (u.UsuarioModulo.filter(e => e.CD_MOD == item.CD_MOD).length > 0) {
				comp.listaModulosSelecionados.push(item);
			} else {
				comp.listaModulos.push(item);
			}
		});

		u.RotinaUsuarioOperacao.forEach(function (item) {

			if (comp.listaRotinaOperacoesSelecionadas.filter(o => o.rotina && o.rotina.CD_ROT == item.CD_ROT).length == 0) {

				let novaRotinaOperacoes = new RotinaOperacoes();
				novaRotinaOperacoes.rotina = comp.listaRotinas.find(o => o.CD_ROT == item.CD_ROT);

				comp.listaOperacoes.forEach(function (op) {

					let operacaoAdicionar = new Operacao;
					operacaoAdicionar.CD_OPR = op.CD_OPR;
					operacaoAdicionar.TX_DSC = op.TX_DSC;
					operacaoAdicionar.selecionada = false;

					novaRotinaOperacoes.operacoes.push(operacaoAdicionar);
				})

				comp.listaRotinaOperacoesSelecionadas.push(novaRotinaOperacoes);
			}
		});

		this.listaRotinaOperacoesSelecionadas.forEach(function (item) {

			item.operacoes.forEach(function (op) {

				if (u.RotinaUsuarioOperacao.filter(o => item.rotina && o.CD_ROT == item.rotina.CD_ROT && o.CD_OPR == op.CD_OPR).length > 0) {
					op.selecionada = true;
				}
			})
		})
	}

	adicionarModulo(modulo, index) {

		this.listaModulosSelecionados.push(modulo);
		this.listaModulos.splice(index, 1);
	}
	removerModulo(modulo, index) {
		this.listaModulos.push(modulo);
		this.listaModulosSelecionados.splice(index, 1);
	}

	adicionarGrupo(grupo, index) {
		this.listaGruposSelecionados.push(grupo);
		this.listaGrupos.splice(index, 1);
	}
	removerGrupo(grupo, index) {
		this.listaGrupos.push(grupo);
		this.listaGruposSelecionados.splice(index, 1);
	}


	filtrarRotinas(cdSrv) {
		return this.listaRotinas.filter(o => o.CD_SRV == cdSrv);
	}

	adicionarRotina() {

		if (this.cdRotSelecionada && !this.listaRotinaOperacoesSelecionadas.find(o=>o.rotina && o.rotina.CD_ROT == this.cdRotSelecionada)) {

			let rotinaOperacoes = new RotinaOperacoes();
			rotinaOperacoes.rotina = this.listaRotinas.find(o => o.CD_ROT == this.cdRotSelecionada);

			this.listaOperacoes.forEach(function (op) {

				let operacaoAdicionar = new Operacao;
				operacaoAdicionar.CD_OPR = op.CD_OPR;
				operacaoAdicionar.TX_DSC = op.TX_DSC;
				operacaoAdicionar.selecionada = false;

				rotinaOperacoes.operacoes.push(operacaoAdicionar);
			})
			this.listaRotinaOperacoesSelecionadas.push(rotinaOperacoes);
		}


	}

	habilitarEdicao() {
		this.modalService.dismissAll();
		this.modoEdicao = true;
		this.titulo = "Editar Usuário"
	}

	excluir() {

		this.modalService.dismissAll();

		let ngbModalOptions: NgbModalOptions = {
			backdrop: 'static',
			keyboard: false
		};
		this.modalService.open(this.modalExcluindo, ngbModalOptions);

		let usuarioExcluir: Usuario = this.usuarioForm.value;

		this.usuarioService.deletarUsuario(usuarioExcluir.CD_USR).subscribe(result => {

			this.modalService.dismissAll();
			this.usuarioService.telaLista = true;
			this.toast.success("Registro excluído com sucesso!", 'Notificação');
			this.cd.markForCheck();
			// this.router.navigate(['/seguranca/usuarios', { excluido: "true" }]);
		})
	}

	removerRotinaSelecionada(index){
		this.listaRotinaOperacoesSelecionadas.splice(index,1);
	}

	//-------------------------------------------------------------------------------------------------
	// Método para carregar as permissões da página----------------------------------------------------
	//-------------------------------------------------------------------------------------------------
	carregarPermissoes(){
		this.permissaoService.getPermissoes(this.auth.idUsuario, this.nomeRotina).subscribe(permissao => {
			this.permissoes = permissao;
			this.cd.markForCheck();
			console.log(this.permissoes);
			this.cd.markForCheck();
		});
	}

	//-------------------------------------------------------------------------------------------------
	// Método para verificar a permissão sobre componente----------------------------------------------
	//-------------------------------------------------------------------------------------------------
	verificarPermissao(acao:string){

		if(this.permissoes === undefined || this.permissoes === null || this.permissoes.length === 0)
		{
			return false;
		}

		var encontrou = this.permissoes.filter(filtro => filtro.TX_DSC === acao);


		if(encontrou === undefined || encontrou === null || encontrou.length === 0)
		{
			return false;
		}
		else
		{
			return true;
		}
	}
}