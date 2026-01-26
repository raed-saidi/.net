using Microsoft.AspNetCore.Identity;
using RealEstateMarketplace.BLL.DTOs;
using RealEstateMarketplace.DAL.Entities;
using RealEstateMarketplace.DAL.Repositories;

namespace RealEstateMarketplace.BLL.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<User> _userManager;
    
    public DashboardService(IUnitOfWork unitOfWork, UserManager<User> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }
    
    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var totalProperties = await _unitOfWork.Properties.CountAsync();
        var activeProperties = await _unitOfWork.Properties.CountAsync(p => p.Status == PropertyStatus.Active);
        var propertiesForSale = await _unitOfWork.Properties.CountAsync(p => p.ListingType == ListingType.Sale && p.Status == PropertyStatus.Active);
        var propertiesForRent = await _unitOfWork.Properties.CountAsync(p => p.ListingType == ListingType.Rent && p.Status == PropertyStatus.Active);
        
        var totalUsers = _userManager.Users.Count();
        
        var inquiryRepo = _unitOfWork.Repository<Inquiry>();
        var totalInquiries = await inquiryRepo.CountAsync();
        var unreadInquiries = await inquiryRepo.CountAsync(i => !i.IsRead);
        
        var recentProperties = await _unitOfWork.Properties.GetLatestPropertiesAsync(5);
        var recentInquiries = await inquiryRepo.FindAsync(i => true);
        
        return new DashboardStatsDto
        {
            TotalProperties = totalProperties,
            ActiveProperties = activeProperties,
            TotalUsers = totalUsers,
            TotalInquiries = totalInquiries,
            UnreadInquiries = unreadInquiries,
            PropertiesForSale = propertiesForSale,
            PropertiesForRent = propertiesForRent,
            RecentProperties = recentProperties.Select(p => new PropertyDto
            {
                Id = p.Id,
                Title = p.Title,
                Price = p.Price,
                City = p.City,
                Status = p.Status.ToString(),
                MainImageUrl = p.MainImageUrl,
                CreatedAt = p.CreatedAt
            }).ToList(),
            RecentInquiries = recentInquiries.OrderByDescending(i => i.CreatedAt).Take(5).Select(i => new InquiryDto
            {
                Id = i.Id,
                Subject = i.Subject,
                SenderName = i.SenderName,
                SenderEmail = i.SenderEmail,
                IsRead = i.IsRead,
                CreatedAt = i.CreatedAt
            }).ToList()
        };
    }
}
