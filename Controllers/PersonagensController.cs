using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RpgApi.Data;
using RpgApi.Models;
using Microsoft.EntityFrameworkCore;

namespace RpgApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PersonagensController : ControllerBase
    {
         private readonly DataContext _context;

        public PersonagensController(DataContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")] //Buscar pelo id
        public async Task<IActionResult> GetSingle(int id)
        {
            try
            {
                Personagem p = await _context.TB_PERSONAGENS
                    .Include(ar => ar.Arma) //Inclui na propriedade Arma do objeto p
                    .Include(us => us.Usuario) //Inclui na propriedade Usuario do objeto p
                    .Include(ph => ph.PersonagemHabilidades)
                        .ThenInclude(h => h.Habilidade) //Inclui na lista de PersonagemHabilidades bilidad                   
                    .FirstOrDefaultAsync(pBusca => pBusca.Id == id);

                return Ok(p);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message + " - " + ex.InnerException);
            }
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> Get()
        {
            try
            {
                List<Personagem> lista = await _context.TB_PERSONAGENS.ToListAsync();
                return Ok(lista);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message + " - " + ex.InnerException);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add(Personagem novoPersonagem)
        {
            try
            {
                if(novoPersonagem.PontosVida > 100)
                    throw new Exception("Pontos de vida não pode ser maior que 100");

                await _context.TB_PERSONAGENS.AddAsync(novoPersonagem);
                await _context.SaveChangesAsync();

                return Ok(novoPersonagem.Id);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message + " - " + ex.InnerException);
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update(Personagem novoPersonagem)
        {
            try
            {
                if(novoPersonagem.PontosVida > 100)
                    throw new Exception("Pontos de vida não pode ser maior que 100");

                _context.TB_PERSONAGENS.Update(novoPersonagem);
                int linhasAfetadas = await _context.SaveChangesAsync();

                return Ok(linhasAfetadas);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message + " - " + ex.InnerException);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                Personagem? pRemover = await _context.TB_PERSONAGENS
                    .FirstOrDefaultAsync(p => p.Id == id);

                _context.TB_PERSONAGENS.Remove(pRemover);
                int linhaAfetadas = await _context.SaveChangesAsync();
                return Ok(linhaAfetadas);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message + " - " + ex.InnerException);
            }
        }

        [HttpPost("DeletePersonagemHabilidade")]
            public async Task<IActionResult> DeleteAsync(PersonagemHabilidade ph)
            {
            
                try
                {
                    PersonagemHabilidade? phRemover = await _context.TB_PERSONAGENS_HABILIDADES
                        .FirstOrDefaultAsync(phBusca => phBusca.PersonagemId == ph.PersonagemId
                        && phBusca.HabilidadeId == ph.HabilidadeId); 
                    if (phRemover == null)
                        throw new System.Exception("Personagem ou habilidade não encontrado.");

                    _context.TB_PERSONAGENS_HABILIDADES.Remove(phRemover);
                    int linhasAfetadas = await _context.SaveChangesAsync();
                    return Ok(linhasAfetadas);
                }
                catch (System.Exception ex)
                {
                    return BadRequest(ex.Message + " - " + ex.InnerException);
                }
            }



        // Restaurar pontos de vida
        [HttpPut("RestaurarPontosVida")]
        public async Task<IActionResult> RestaurarPontosVidaAsync(Personagem p)
        {
            try
            {
                int linhasAfetadas = 0;

                Personagem? pEncontrado =
                    await _context.TB_PERSONAGENS.FirstOrDefaultAsync(pBusca => pBusca.Id == p.Id);

                if (pEncontrado == null)
                    throw new System.Exception("Personagem não encontrado.");

                pEncontrado.PontosVida = 100;

                bool atualizou = await TryUpdateModelAsync<Personagem>(pEncontrado, "p",
                    pAtualizar => pAtualizar.PontosVida);

                // EF vai detectar e atualizar apenas as colunas que foram alteradas.
                if (atualizou)
                    linhasAfetadas = await _context.SaveChangesAsync();

                return Ok(linhasAfetadas);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //Método para alteração da foto
        [HttpPut("AtualizarFoto")]
        public async Task<IActionResult> AtualizarFotoAsync(Personagem p)
        {
            try
            {
                Personagem? personagem = await _context.TB_PERSONAGENS
                    .FirstOrDefaultAsync(x => x.Id == p.Id);

                if (personagem == null)
                    throw new System.Exception("Personagem não encontrado.");

                personagem.FotoPersonagem = p.FotoPersonagem;

                var attach = _context.Attach(personagem);
                attach.Property(x => x.Id).IsModified = false;
                attach.Property(x => x.FotoPersonagem).IsModified = true;

                int linhasAfetadas = await _context.SaveChangesAsync();
                return Ok(linhasAfetadas);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Zerar ranking de um personagem
        [HttpPut("ZerarRanking")]
        public async Task<IActionResult> ZerarRankingAsync(Personagem p)
        {
            try
            {
                Personagem? pEncontrado =
                    await _context.TB_PERSONAGENS.FirstOrDefaultAsync(pBusca => pBusca.Id == p.Id);

                if (pEncontrado == null)
                    throw new System.Exception("Personagem não encontrado.");

                pEncontrado.Disputas = 0;
                pEncontrado.Vitorias = 0;
                pEncontrado.Derrotas = 0;

                int linhasAfetadas = 0;

                bool atualizou = await TryUpdateModelAsync<Personagem>(pEncontrado, "p",
                    pAtualizar => pAtualizar.Disputas,
                    pAtualizar => pAtualizar.Vitorias,
                    pAtualizar => pAtualizar.Derrotas);

                // EF vai detectar e atualizar apenas as colunas que foram alteradas.
                if (atualizou)
                    linhasAfetadas = await _context.SaveChangesAsync();

                return Ok(linhasAfetadas);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Zerar ranking geral e restaurar vidas geral
        [HttpPut("ZerarRankingRestaurarVidas")]
        public async Task<IActionResult> ZerarRankingRestaurarVidasAsync()
        {
            try
            {
                List<Personagem> lista =
                    await _context.TB_PERSONAGENS.ToListAsync();

                foreach (Personagem p in lista)
                {
                    await ZerarRankingAsync(p);
                    await RestaurarPontosVidaAsync(p);
                }

                return Ok();
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Buscar os personagens de acordo com o Id de um usuário
        [HttpGet("GetByUser/{userId}")]
        public async Task<IActionResult> GetByUserAsync(int userId)
        {
            try
            {
                List<Personagem> lista = await _context.TB_PERSONAGENS
                    .Where(u => u.Usuario!.Id == userId)
                    .ToListAsync();

                return Ok(lista);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Verifica o perfil do usuário: Admin traz todos; Jogador traz só os que ele cadastrou
        [HttpGet("GetByPerfil/{userId}")]
        public async Task<IActionResult> GetByPerfilAsync(int userId)
        {
            try
            {
                Usuario? usuario = await _context.TB_USUARIOS
                    .FirstOrDefaultAsync(x => x.Id == userId);

                if (usuario == null)
                    throw new System.Exception("Usuário não encontrado.");

                List<Personagem> lista = new List<Personagem>();

                if (usuario.Perfil == "Admin")
                    lista = await _context.TB_PERSONAGENS.ToListAsync();
                else
                    lista = await _context.TB_PERSONAGENS
                        .Where(p => p.Usuario!.Id == userId).ToListAsync();

                return Ok(lista);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Busca aproximada do nome do personagem
        [HttpGet("GetByNomeAproximado/{nomePersonagem}")]
        public async Task<IActionResult> GetByNomeAproximado(string nomePersonagem)
        {
            try
            {
                List<Personagem> lista = await _context.TB_PERSONAGENS
                    .Where(p => p.Nome.ToLower().Contains(nomePersonagem.ToLower()))
                    .ToListAsync();

                return Ok(lista);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
