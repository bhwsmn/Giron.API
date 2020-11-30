using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Giron.API.Entities;
using Giron.API.Models.Input;
using Giron.API.Models.Output;
using Giron.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Giron.API.Controllers
{
    [ApiController]
    [Route("/domains")]
    public class DomainsController : ControllerBase
    {
        private readonly IDomainRepository _domainRepository;
        private readonly IMapper _mapper;

        public DomainsController(IDomainRepository domainRepository, IMapper mapper)
        {
            _domainRepository = domainRepository;
            _mapper = mapper;
        }
        
        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<DomainOutputDto>> CreateDomainAsync(DomainCreateDto domainCreateDto)
        {
            if (await _domainRepository.DomainExistsAsync(domainCreateDto.Name))
            {
                return Conflict();
            }

            var domain = await _domainRepository.CreateDomainAsync(_mapper.Map<Domain>(domainCreateDto));

            return CreatedAtAction(
                actionName: nameof(GetDomainByNameAsync),
                routeValues: new {name = domain.Name},
                value: _mapper.Map<DomainOutputDto>(domain)
            );
        }
        
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<DomainOutputDto>>> GetAllDomainsAsync()
        {
            var domains = await _domainRepository.GetAllDomainsAsync();
        
            return _mapper.Map<List<DomainOutputDto>>(domains);
        }

        [HttpHead("{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDomainByNameAsync(string name)
        {
            var domainExists = await _domainRepository.DomainExistsAsync(name);

            if (!domainExists)
            {
                return NotFound();
            }

            return Ok();
        }

    }
}