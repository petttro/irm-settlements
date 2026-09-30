using IRM.Settlements.Application.Abstractions;
using IRM.Settlements.Application.Abstractions.Repositories;
using IRM.Settlements.Application.Abstractions.Services;
using IRM.Settlements.Application.Integrations.IRM;
using IRM.Settlements.Domain.Entities;

namespace IRM.Settlements.Application.Services;

public class AppealService : IAppealService
{
    private readonly IAppealRepository _appealRepository;
    private readonly IServiceCompanyRepository _serviceCompanyRepository;
    private readonly IServiceCenterRepository _serviceCenterRepository;
    private readonly IMvzRepository _mvzRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AppealService(
        IAppealRepository appealRepository,
        IServiceCompanyRepository serviceCompanyRepository,
        IServiceCenterRepository serviceCenterRepository,
        IMvzRepository mvzRepository,
        IUnitOfWork unitOfWork)
    {
        _appealRepository = appealRepository;
        _serviceCompanyRepository = serviceCompanyRepository;
        _serviceCenterRepository = serviceCenterRepository;
        _mvzRepository = mvzRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task AddOrUpdateAsync(AppealUpdatedEvent updatedEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(updatedEvent);

        var model = updatedEvent.AppealUpdatedMessage;

        await UpsertServiceCompany(model, cancellationToken);
        await UpsertServiceCenter(model, cancellationToken);
        await UpsertMvz(model, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        await UpsertAppeal(model, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);
    }

    private async Task UpsertAppeal(AppealUpdatedMessage message, CancellationToken cancellationToken)
    {
        var appeal = AppealUpdatedMessage.ToEntity(message);
        await _appealRepository.UpsertAsync(appeal, cancellationToken);
    }

    private async Task UpsertServiceCenter(AppealUpdatedMessage message, CancellationToken cancellationToken)
    {
        var serviceCenter = new ServiceCenter
        {
            Id = message.Snapshot.ResolverId,
            ExternalId = message.Snapshot.ResolverExternalId,
            Name = message.Snapshot.ResolverNameResolver,
            ServiceCompanySapId = message.Snapshot.EnterpriseId,
            UpdatedAt = message.CreatedAt
        };
        await _serviceCenterRepository.UpsertAsync(serviceCenter, cancellationToken);
    }

    private async Task UpsertServiceCompany(AppealUpdatedMessage message, CancellationToken cancellationToken)
    {
        var serviceCompany = new ServiceCompany
        {
            Id = message.Snapshot.ServiceCompanyId,
            SapId = message.Snapshot.EnterpriseId,
            Name = message.Snapshot.ServiceCompanyName,
            UpdatedAt = message.CreatedAt
        };
        await _serviceCompanyRepository.UpsertAsync(serviceCompany, cancellationToken);
    }

    private async Task UpsertMvz(AppealUpdatedMessage message, CancellationToken cancellationToken)
    {
        var mvzItem = new MvzItem
        {
            Id = message.Snapshot.ShopName,
            UpdatedAt = message.CreatedAt
        };
        await _mvzRepository.UpsertAsync(mvzItem, cancellationToken);
    }
}
