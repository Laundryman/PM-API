using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlanMatr_API.Controllers.planm;
using PMApplication.Entities.OrderAggregate;
using PMApplication.Interfaces.ServiceInterfaces;
using PMApplication.Services;
using PMApplication.Specifications.Filters;
using System.Net;
using PMApplication.Enums;
using PlanMatr_API.Extensions;
using System.Linq;
using PMApplication.Dtos;
using PMApplication.Entities;
using PMApplication.Entities.CountriesAggregate;

namespace PlanMatr_API.Controllers
{
    public class OrdersApiController : ControllerBase
    {

        private readonly IMapper _mapper;
        private readonly ILogger<EditPlanApiController> _logger;
        private readonly IAIdentityService _identityService;
        private readonly IBrandService _brandService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IPlanogramService _planogramService;
        private readonly ICountryService _countryService;
        private readonly ILmAuditService _auditService;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        public OrdersApiController(IMapper mapper, ILogger<EditPlanApiController> logger, IAIdentityService identityService, IBrandService brandService, IOrderService orderService, IProductService productService, IPlanogramService planogramService, ICountryService countryService, ILmAuditService auditService, IConfiguration config, IWebHostEnvironment env)
        {
            _mapper = mapper;
            _logger = logger;
            _identityService = identityService;
            _brandService = brandService;
            _orderService = orderService;
            _productService = productService;
            _planogramService = planogramService;
            _countryService = countryService;
            _auditService = auditService;
            _config = config;
            _env = env;
        }

        #region API Methods
        [HttpGet]
        [Route("api/v2/order/getOpen/{brandId}/{planogramId}")]
        public async Task<IActionResult> GetOpenOrder(int brandId, int planogramId)
        {
            //we might need this brandId when we have other brands with a shop
            var planogram = await _planogramService.GetPlanogram(planogramId);
            var countryId = planogram.CountryId;
            var filter = new OrderFilter
            {
                CountryId = countryId ?? 0,
                OrderStatus = (int)OrderStatusEnum.Open
            };
            var order = _orderService.GetOrders(filter);
            if (order == null)
            {
                var message = "There is no open order for this planogram's country";

                if (countryId != null)
                {
                    var country = await _countryService.GetCountry(countryId.Value);
                    var countryName = country != null ? country.Name : "Unknown";
                    message = "There is no open order for this planogram's country (" + countryName + ")";
                }

                return BadRequest(message);
            }

            return Ok(order);
        }

        [HttpGet]
        [Route("api/v2/order/getOpenOrders/{brandId}/{planogramId}")]
        public async Task<IActionResult> GetOpenOrders(int brandId, int planogramId)
        {
            //we might need this brandId when we have other brands with a shop
            var planogram = await _planogramService.GetPlanogram(planogramId);
            var countryId = planogram.CountryId;
            var filter = new OrderFilter
            {
                CountryId = countryId ?? 0,
                OrderStatus = (int)OrderStatusEnum.Open
            };

            var orders = await _orderService.GetOrders(filter);
            if (!orders.Any())
            {
                string message = "There are no open orders for this planogram's country";

                if (countryId != null)
                {
                    var country = await _countryService.GetCountry(countryId.Value);
                    message = "There are no open orders for this planogram's country (" + country.Name + ")";
                }



                return BadRequest(message);
            }

            var openOrders = orders.Select(o => new OpenOrder { OrderId = o.Id, OrderTitle = o.OrderTitle }).ToList();
                
            return Ok(openOrders);
        }


        [HttpGet]
        [Route("api/v2/order/addToOrder/{orderId}/{planogramId}/{quantity}/{userId}/{isFullPlano}/{brandid}")]
        public async Task<IActionResult> AddToOrder(int orderId, int planogramId, int quantity, string userId, bool isFullPlano, int brandid)
        {
            //var currentUser = HttpContext.Current.User;
            var currentUseruserProfile = await this.MappedUser(_identityService);

            try
            {


                var userProfile = await this.MappedUser(_identityService);

                _orderService.AddPartsToOrder(orderId, planogramId, quantity, userId, userProfile.UserName, isFullPlano);
                //LOG SUCCESSFULL Planogram Save HERE

                _logger.LogInformation(Request.Headers.ToString());


                //Audit the action
                var audit = new LMAuditLog
                {
                    UserId = userProfile.Id,
                    Date = DateTime.Now,
                    BrandId = brandid,
                    Roles = userProfile.RoleIds,
                    UserName = userProfile.DisplayName,
                    Action = (int)LogActionEnum.EditOrder,
                    Message = userProfile.DisplayName + " edited order with Id " + orderId.ToString(),
                    OrderId = orderId
                };
                _auditService.AuditEvent(audit);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("api/v2/order/updateOrderItemQuantity/{orderItemId:int}/{quantity:int}")]
        public async Task<IActionResult> UpdateOrderItemQuantity(int orderItemId, int quantity, int brandId)
        {
            //var response = new ApiResponseModel();
            var currentUser = await this.MappedUser(_identityService);

            try
            {
                var orderItem = await _orderService.GetOrderItem(orderItemId);

                if (orderItem == null)
                {
                    throw new ArgumentException("Order item could not be found");
                }

                orderItem.Quantity = quantity;
                _orderService.SaveOrderItem();

                //LOG SUCCESSFULL Planogram Save HERE
                var userProfile = await this.MappedUser(_identityService);
                string userId = userProfile.Id;
                //Get The Client Application Info


                //SecurityLogger.OrderFormat("User edited Order with id {0} from IP address {1}", userId, brandId,
                //    orderItem.OrderId, userProfile.UserName, (int)LogActionEnum.EditOrder, userId,
                //    HttpContext.Current.Request.UserHostAddress);

                //Audit the action
                var audit = new LMAuditLog
                {
                    UserId = userProfile.Id,
                    Date = DateTime.Now,
                    BrandId = brandId,
                    Roles = userProfile.RoleIds,
                    UserName = userProfile.DisplayName,
                    Action = (int)LogActionEnum.EditOrder,
                    Message = userProfile.DisplayName + " edited order with Id " + orderItem.OrderId.ToString(),
                    OrderId = orderItem.Id
                };

                _auditService.AuditEvent(audit);

                var order = await _orderService.GetOrder(orderItem.OrderId);

                var orderModel = await BuildFullOrder(order);

                return Ok(orderModel);
                //return JsonContent(response);

            }
            catch (Exception e)
            {

                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }
        #endregion
        #region private methods
        private async Task<OrderDto> BuildFullOrder(Order order)
        {
            var imageDomain = _config["AppSettings:cassette-photo-url"];

            var orderModel = _mapper.Map<OrderDto>(order);

            //var filter = new OrderItemFilter { OrderId = order.Id };
            var allOrderItems = await _orderService.GetOrderItems(order.Id);

            orderModel.HasLegacyItems = allOrderItems.Any(x => !x.Shoppable);

            //allOrderItems = allOrderItems.Where(x => x.Shoppable).ToList();
            foreach (var orderItemInfo in allOrderItems)
            {
                orderItemInfo.PackShotImageSrc = imageDomain + orderItemInfo.PackShotImageSrc;
            }

            var totalQuantity = allOrderItems.Sum(x => x.Quantity);
            var totalPrice = allOrderItems.Sum(x => x.Price * x.Quantity);

            orderModel.TotalPrice = totalPrice;
            orderModel.TotalQuantity = totalQuantity;

            orderModel.IndividualOrderItems = allOrderItems.Where(x => x.PlanogramId == null);


            var planoModels = new List<OrderPlanogramDto>();

            // partial planos
            var partialPlanoIds = allOrderItems.Where(x => x.PlanogramId != null && (!x.IsFullPlano.HasValue || !x.IsFullPlano.Value))
                .Select(x => x.PlanogramId.Value).Distinct();

            var partialPlanos = partialPlanoIds.Select( x =>  _planogramService.GetPlanogram(x).Result);

            var partialPlanoModels = partialPlanos.Select( x => new OrderPlanogramDto
            {
                PlanogramId = x.Id,
                Name = x.Name,
                IsFullPlano = false
            });

            foreach (var planogram in partialPlanoModels)
            {
                var orderItems = allOrderItems.Where(x => x.PlanogramId == planogram.PlanogramId && (!x.IsFullPlano.HasValue || !x.IsFullPlano.Value));
                planogram.OrderItems = orderItems;
                planogram.PlanogramTotalValue = planogram.OrderItems.Sum(item => (item.Price * item.Quantity));
            }



            // full planos
            var fullPlanoIds = allOrderItems.Where(x => x.PlanogramId != null && (x.IsFullPlano.HasValue && x.IsFullPlano.Value))
                .Select(x => x.PlanogramId.Value).Distinct();

            var fullPlanos = fullPlanoIds.Select(x => _planogramService.GetPlanogram(x).Result);

            var fullPlanoModels = fullPlanos.Select(x => new OrderPlanogramDto
            {
                PlanogramId = x.Id,
                Name = x.Name,
                ClusterName = x.Cluster.Name,
                ClusterPartNumber = x.Cluster.ClusterPartNumber,
                StandName = x.Stand.Name,
                StandPartNumber = x.Stand.StandAssemblyNumber,
                IsFullPlano = true
            }).ToList();

            foreach (var planogram in fullPlanoModels)
            {
                var orderItems = allOrderItems.Where(x => x.PlanogramId == planogram.PlanogramId && (x.IsFullPlano.HasValue && x.IsFullPlano.Value));
                planogram.OrderItems = orderItems;
                planogram.PlanogramTotalValue = planogram.OrderItems.Sum(item => (item.Price * item.Quantity));
            }

            planoModels.AddRange(partialPlanoModels);
            planoModels.AddRange(fullPlanoModels);

            orderModel.Planograms = planoModels;

            return orderModel;
        }

        #endregion
    }
}
