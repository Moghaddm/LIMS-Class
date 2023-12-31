using LIMS.Application.DTOs;
using LIMS.Domain.IRepositories;
using LIMS.Domain.Entities;
using LIMS.Application.Models;
using LIMS.Application.Mappers;
using Microsoft.Extensions.Logging;
using LIMS.Application.Services.Http;

namespace LIMS.Application.Services.Database;

public class ServerServiceImpl
{
    private readonly IServerRepository _servers;
    private readonly ILogger<ServerServiceImpl> _logger;
    private readonly ServerActiveService _activeService;
    private readonly IUnitOfWork _unitOfWork;

    public ServerServiceImpl(
        ServerActiveService activeService,
        IServerRepository servers,
        IUnitOfWork unitOfWork,
        ILogger<ServerServiceImpl> logger) => (_servers, _activeService, _unitOfWork, _logger) = (servers, activeService, unitOfWork, logger);

    public async ValueTask<OperationResult<bool>> CanJoinServer(long id)
    {
        try
        {
            var server = await _servers.GetByIdAsync(id);

            if (server is null)
                return OperationResult<bool>.OnFailed("Expected Server Not Found.");

            if (await _servers.CanJoinOnServerAsync(server))
            {
                _logger.LogWarning($"{server.ServerUrl} is Full Capacity and Cannot Join On That Anymore.");

                return OperationResult<bool>.OnFailed("Cant Join On This Server. It is Full!");
            }

            return OperationResult<bool>.OnSuccess(true);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, exception.Message);

            return OperationResult<bool>.OnException(exception);
        }
    }

    public async Task<OperationResult> UpdateServer(long id, ServerAddEditDto server)
    {
        if (server is null)
            return OperationResult<Server>.OnFailed("Server Cannot Be Null.");

        var newServer = await _servers.GetByIdAsync(id);

        if (newServer is null)
            return OperationResult<Server>.OnFailed("Server Not Found");

        await newServer.UpdateServer(server.ServerUrl, server.ServerSecret, server.ServerLimit);

        await _unitOfWork.SaveChangesAsync();

        return new OperationResult();
    }

    public async ValueTask<OperationResult<long>> CreateServer(ServerAddEditDto serverAddEditDto)
    {
        if (serverAddEditDto is null)
            return OperationResult<long>.OnFailed("Please Send Valid Server Entity.");

        var serverId = await _servers.CreateAsync(ServerDtoMapper.Map(serverAddEditDto));

        await _unitOfWork.SaveChangesAsync();

        return OperationResult<long>.OnSuccess(serverId);
    }

    public async ValueTask<OperationResult<ServerAddEditDto>> GetMostCapableServer()
    {
        try
        {
            var activeServers = await _servers.GetAllActiveAsync();

            var orderedServers = await _servers.GetAllOrderedDescendingAsync(activeServers);

            var capableServer = await _servers.GetFirstAsync(orderedServers);

            var serverDto = ServerDtoMapper.Map(capableServer);

            return OperationResult<ServerAddEditDto>.OnSuccess(serverDto);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, exception.Message);

            return OperationResult<ServerAddEditDto>.OnException(exception);
        }
    }

    public async ValueTask<OperationResult> DeleteServer(long Id)
    {
        await _servers.DeleteAsync(Id);

        return new OperationResult();
    }

    public async ValueTask<OperationResult<ServerAddEditDto>> GetServer(long Id)
    {
        var server = ServerDtoMapper.Map(await _servers.GetByIdAsync(Id));

        return OperationResult<ServerAddEditDto>.OnSuccess(server);
    }

    public async ValueTask<OperationResult<List<ServerAddEditDto>>> GetAllServers()
    {
        var servers = await _servers.GetAllAsync();

        var serversDto = new List<ServerAddEditDto>();

        foreach (var server in servers)
            serversDto.Add(ServerDtoMapper.Map(server));

        return OperationResult<List<ServerAddEditDto>>.OnSuccess(serversDto);
    }

    public async Task<OperationResult> UpdateServersForActivate()
    {
        try
        {
            var servers = await _servers.GetAllAsync();

            var checkActiveServers = await _activeService.SetActiveServers(servers);

            if (!checkActiveServers.Success)
                return OperationResult.OnFailed(checkActiveServers.OnFailedMessage);

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"All Servers Which Being Down are Checked And Activated.");

            return new OperationResult();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, exception.Message);

            return OperationResult.OnFailed(exception.Message);
        }
    }

    public async Task<OperationResult<bool>> UpdateServerCheckForBeingDown(string url)
    {
        try
        {
            var server = await _servers.GetByUrlAsync(url);

            var isDown = await _activeService.IsServerDown(server.ServerUrl);

            if (!isDown.Result)
            {
                _logger.LogInformation($"{server.ServerUrl} Is Active And Ready For Joining On That.");

                return OperationResult<bool>.OnSuccess(false);
            }

            server.SetDownServer();

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"{server.ServerUrl} Is Downed and Cannot Create Meeting Anymore.");

            return OperationResult<bool>.OnSuccess(true);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, exception.Message);

            return OperationResult<bool>.OnFailed(exception.Message);
        }
    }
}