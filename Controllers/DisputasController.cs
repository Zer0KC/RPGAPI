using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RpgApi.Data;
using RpgApi.Models;

namespace RpgApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DisputasController : ControllerBase
    {
        private readonly DataContext _context;

        public DisputasController(DataContext context)
        {
            _context = context;
        }

        [HttpPost("Arma")]
        public async Task<IActionResult> AtaqueComArmaAsync(Disputa d)
        {
            try
            {
                Personagem? atacante = await _context.TB_PERSONAGENS
                    .Include(p => p.Arma)
                    .FirstOrDefaultAsync(p => p.Id == d.AtacanteId);

                Personagem? oponente = await _context.TB_PERSONAGENS
                    .FirstOrDefaultAsync(p => p.Id == d.OponenteId);

                if (atacante == null || oponente == null)
                    return BadRequest("Personagem não encontrado.");

                if (atacante.Arma == null)
                    return BadRequest("O atacante não possui arma.");

                Random random = new Random();

                int dano = atacante.Arma.Dano + random.Next(atacante.Forca);

                dano -= random.Next(oponente.Defesa);

                if (dano > 0)
                {
                    oponente.PontosVida -= dano;

                    if (oponente.PontosVida <= 0)
                    {
                        d.Narracao = $"{oponente.Nome} foi derrotado.";
                    }
                }
                else
                {
                    d.Narracao = $"{oponente.Nome} defendeu o ataque.";
                }

                _context.TB_PERSONAGENS.Update(oponente);

                StringBuilder dados = new StringBuilder();

                dados.AppendFormat("Atacante: {0}. ", atacante.Nome);
                dados.AppendFormat("Oponente: {0}. ", oponente.Nome);
                dados.AppendFormat("Pontos de vida do atacante: {0}. ", atacante.PontosVida);
                dados.AppendFormat("Pontos de vida do oponente: {0}. ", oponente.PontosVida);
                dados.AppendFormat("Arma utilizada: {0}. ", atacante.Arma.Nome);
                dados.AppendFormat("Dano: {0}. ", dano);

                d.Narracao += dados.ToString();
                d.DataDisputa = DateTime.Now;

                _context.TB_DISPUTAS.Add(d);

                await _context.SaveChangesAsync();

                return Ok(d);
            }
            catch (Exception ex)
                {
                    return BadRequest(new { message = "Erro ao processar a disputa: " + (ex.InnerException?.Message ?? ex.Message) });
                }
        }

        [HttpPost("Habilidade")]
        public async Task<IActionResult> AtaqueComHabilidadeAsync(Disputa d)
        {
            try
            {
                Personagem? atacante = await _context.TB_PERSONAGENS
                    .FirstOrDefaultAsync(p => p.Id == d.AtacanteId);

                Personagem? oponente = await _context.TB_PERSONAGENS
                    .FirstOrDefaultAsync(p => p.Id == d.OponenteId);

                if (atacante == null || oponente == null)
                    return BadRequest("Personagem não encontrado.");

                PersonagemHabilidade? ph = await _context.TB_PERSONAGENS_HABILIDADES
                    .Include(ph => ph.Habilidade)
                    .FirstOrDefaultAsync(ph =>
                        ph.PersonagemId == d.AtacanteId &&
                        ph.HabilidadeId == d.HabilidadeId);

                if (ph == null)
                {
                    d.Narracao = $"{atacante.Nome} não possui essa habilidade.";
                }
                else
                {
                    Random random = new Random();

                    int dano = ph.Habilidade.Dano +
                        random.Next(atacante.Inteligencia);

                    dano -= random.Next(oponente.Defesa);

                    if (dano > 0)
                    {
                        oponente.PontosVida -= dano;

                        if (oponente.PontosVida <= 0)
                        {
                            d.Narracao = $"{oponente.Nome} foi derrotado.";
                        }
                    }
                    else
                    {
                        d.Narracao = $"{oponente.Nome} defendeu o ataque.";
                    }

                    _context.TB_PERSONAGENS.Update(oponente);

                    StringBuilder dados = new StringBuilder();

                    dados.AppendFormat("Atacante: {0}. ", atacante.Nome);
                    dados.AppendFormat("Oponente: {0}. ", oponente.Nome);
                    dados.AppendFormat("Pontos de vida do atacante: {0}. ", atacante.PontosVida);
                    dados.AppendFormat("Pontos de vida do oponente: {0}. ", oponente.PontosVida);
                    dados.AppendFormat("Habilidade utilizada: {0}. ", ph.Habilidade.Nome);
                    dados.AppendFormat("Dano: {0}. ", dano);

                    d.Narracao += dados.ToString();
                }

                d.DataDisputa = DateTime.Now;

                _context.TB_DISPUTAS.Add(d);

                await _context.SaveChangesAsync();

                return Ok(d);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Erro ao processar a disputa: {ex.Message}" });
            }
        }

        [HttpPost("DisputaEmGrupo")]
        public async Task<IActionResult> DisputaEmGrupoAsync(Disputa d)
        {
            try
            {
                if (d.ListaIdPersonagens == null || d.ListaIdPersonagens.Count < 2)
                    return BadRequest("Informe ao menos dois Ids de personagens em ListaIdPersonagens.");

                d.Resultados = new List<string>(); // Instancia a lista de resultados
                d.Narracao = string.Empty;

                // Busca no banco os personagens informados, incluindo armas e habilidades
                List<Personagem> personagens = await _context.TB_PERSONAGENS
                    .Include(p => p.Arma)
                    .Include(p => p.PersonagemHabilidades)
                        .ThenInclude(ph => ph.Habilidade)
                    .Where(p => d.ListaIdPersonagens.Contains(p.Id))
                    .ToListAsync();

                if (personagens.Count < 2)
                    return BadRequest("Foram encontrados menos de dois personagens para os Ids informados.");

                // Contagem de personagens vivos
                int qtdPersonagensVivos = personagens.Count(p => p.PontosVida > 0);

                // Enquanto houver mais de um personagem vivo, haverá disputa
                while (qtdPersonagensVivos > 1)
                {
                    // Personagens com pontos de vida positivos
                    List<Personagem> vivos = personagens.Where(p => p.PontosVida > 0).ToList();

                    // Trava de segurança: se ninguém vivo tem arma ou habilidade, a disputa nunca terminaria
                    if (!vivos.Any(p => p.Arma != null || p.PersonagemHabilidades.Count > 0))
                    {
                        string semAtaque = "Disputa encerrada: nenhum dos personagens restantes possui arma ou habilidade.";
                        d.Narracao += semAtaque;
                        d.Resultados.Add(semAtaque);
                        break;
                    }

                    // Sorteia o atacante entre os vivos
                    Personagem atacante = vivos[Random.Shared.Next(vivos.Count)];
                    d.AtacanteId = atacante.Id;

                    // Sorteia o oponente entre os vivos, exceto o atacante
                    List<Personagem> oponentes = vivos.Where(p => p.Id != atacante.Id).ToList();
                    Personagem oponente = oponentes[Random.Shared.Next(oponentes.Count)];
                    d.OponenteId = oponente.Id;

                    // Variáveis redefinidas a cada passagem do while
                    int dano = 0;
                    string ataqueUsado = string.Empty;
                    string resultado = string.Empty;

                    bool temArma = atacante.Arma != null;
                    bool temHabilidade = atacante.PersonagemHabilidades.Count > 0;

                    // Sorteio: 0 = ataque com arma, 1 = ataque com habilidade
                    bool ataqueUsaArma = Random.Shared.Next(2) == 0;

                    if (temArma && (ataqueUsaArma || !temHabilidade))
                    {
                        // Mesma lógica do ataque com arma
                        dano = atacante.Arma!.Dano + Random.Shared.Next(atacante.Forca);
                        dano -= Random.Shared.Next(oponente.Defesa);

                        if (dano > 0)
                            oponente.PontosVida -= dano;
                        else
                            dano = 0;

                        ataqueUsado = atacante.Arma.Nome;
                    }
                    else if (temHabilidade)
                    {
                        // Sorteia uma das habilidades do atacante
                        int sorteioHabilidade = Random.Shared.Next(atacante.PersonagemHabilidades.Count);
                        Habilidade habilidadeEscolhida = atacante.PersonagemHabilidades[sorteioHabilidade].Habilidade!;

                        dano = habilidadeEscolhida.Dano + Random.Shared.Next(atacante.Inteligencia);
                        dano -= Random.Shared.Next(oponente.Defesa);

                        if (dano > 0)
                            oponente.PontosVida -= dano;
                        else
                            dano = 0;

                        ataqueUsado = habilidadeEscolhida.Nome;
                    }

                    // (A) Verifica se o ataque teve resultado (não existe se não houver arma nem habilidade)
                    if (!string.IsNullOrEmpty(ataqueUsado))
                    {
                        resultado = $"{atacante.Nome} atacou {oponente.Nome} usando {ataqueUsado} com o dano {dano}.";

                        // (B) Incrementa os dados de disputa do atacante e do oponente
                        atacante.Disputas++;
                        oponente.Disputas++;

                        if (oponente.PontosVida <= 0)
                        {
                            atacante.Vitorias++;
                            oponente.Derrotas++;
                            resultado += $" {oponente.Nome} foi derrotado.";
                        }

                        d.Narracao += resultado + " ";
                        d.Resultados.Add(resultado);

                        // (C) Salva a disputa no banco. Usa um registro novo por rodada, para
                        // não repetir a chave (Id) do objeto d e não estourar o varchar(200) da narração.
                        Disputa registro = new Disputa
                        {
                            AtacanteId = atacante.Id,
                            OponenteId = oponente.Id,
                            DataDisputa = DateTime.Now,
                            Narracao = resultado
                        };
                        _context.TB_DISPUTAS.Add(registro);
                        await _context.SaveChangesAsync();
                    }

                    // (D) Conta os personagens que ainda têm pontos de vida positivos
                    qtdPersonagensVivos = personagens.Count(p => p.PontosVida > 0);

                    // (E) Se só restou um, ele é o campeão
                    if (qtdPersonagensVivos == 1)
                    {
                        // (F) Mensagem final e break para interromper o while
                        string resultadoFinal = $"{atacante.Nome.ToUpper()} é CAMPEÃO com {atacante.PontosVida} pontos de vida restantes!";
                        d.Narracao += resultadoFinal;
                        d.Resultados.Add(resultadoFinal);
                        break;
                    }
                }

                // Após o while: atualiza pontos de vida, disputas, vitórias e derrotas de todos os personagens
                _context.TB_PERSONAGENS.UpdateRange(personagens);
                await _context.SaveChangesAsync();

                return Ok(d);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao processar a disputa em grupo: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }

        [HttpDelete("ApagarDisputas")]
        public async Task<IActionResult> DeleteAsync()
        {
            try
            {
                List<Disputa> disputas = await _context.TB_DISPUTAS.ToListAsync();

                _context.TB_DISPUTAS.RemoveRange(disputas);
                await _context.SaveChangesAsync();

                return Ok("Disputas apagadas");
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("Listar")]
        public async Task<IActionResult> ListarAsync()
        {
            try
            {
                List<Disputa> disputas =
                    await _context.TB_DISPUTAS.ToListAsync();

                return Ok(disputas);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("PersonagemRandom")]
        public async Task<IActionResult> Sorteio()
        {
            List <Personagem> personagens =
                await _context.TB_PERSONAGENS.ToListAsync();

            int sorteio = new Random().Next(0, personagens.Count);

            Personagem p = personagens[sorteio];

            string msg = string.Format("Nº Sorteado: {0}. Personagem: {1}.", sorteio, p.Nome);
            
            return Ok(new { message = msg });
        }
    }
}
