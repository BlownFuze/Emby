﻿using MediaBrowser.Common.Extensions;
using MediaBrowser.Controller.Connect;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Net;
using MediaBrowser.Model.Connect;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaBrowser.Api
{
    [Route("/Users/{Id}/Connect/Link", "POST", Summary = "Creates a Connect link for a user")]
    [Authenticated(Roles = "Admin")]
    public class CreateConnectLink : IReturn<UserLinkResult>
    {
        [ApiMember(Name = "Id", Description = "User Id", IsRequired = true, DataType = "string", ParameterType = "query", Verb = "POST")]
        public string Id { get; set; }

        [ApiMember(Name = "ConnectUsername", Description = "Connect username", IsRequired = true, DataType = "string", ParameterType = "query", Verb = "POST")]
        public string ConnectUsername { get; set; }
    }

    [Route("/Users/{Id}/Connect/Link", "DELETE", Summary = "Removes a Connect link for a user")]
    [Authenticated(Roles = "Admin")]
    public class DeleteConnectLink : IReturnVoid
    {
        [ApiMember(Name = "Id", Description = "User Id", IsRequired = true, DataType = "string", ParameterType = "query", Verb = "DELETE")]
        public string Id { get; set; }
    }

    [Route("/Connect/Invite", "POST", Summary = "Creates a Connect link for a user")]
    [Authenticated(Roles = "Admin")]
    public class CreateConnectInvite : IReturn<UserLinkResult>
    {
        [ApiMember(Name = "ConnectUsername", Description = "Connect username", IsRequired = true, DataType = "string", ParameterType = "query", Verb = "POST")]
        public string ConnectUsername { get; set; }

        [ApiMember(Name = "SendingUserId", Description = "Sending User Id", IsRequired = true, DataType = "string", ParameterType = "query", Verb = "DELETE")]
        public string SendingUserId { get; set; }
    }


    [Route("/Connect/Pending", "GET", Summary = "Creates a Connect link for a user")]
    [Authenticated(Roles = "Admin")]
    public class GetPendingGuests : IReturn<List<ConnectAuthorization>>
    {
    }


    [Route("/Connect/Pending", "DELETE", Summary = "Deletes a Connect link for a user")]
    [Authenticated(Roles = "Admin")]
    public class DeleteAuthorization : IReturnVoid
    {
        [ApiMember(Name = "Id", Description = "Authorization Id", IsRequired = true, DataType = "string", ParameterType = "query", Verb = "DELETE")]
        public string Id { get; set; }
    }

    [Route("/Connect/Exchange", "GET", Summary = "Gets the corresponding local user from a connect user id")]
    [Authenticated]
    public class GetLocalUser : IReturn<ConnectAuthenticationExchangeResult>
    {
        [ApiMember(Name = "ConnectUserId", Description = "ConnectUserId", IsRequired = true, DataType = "string", ParameterType = "query", Verb = "POST")]
        public string ConnectUserId { get; set; }
    }

    public class ConnectService : BaseApiService
    {
        private readonly IConnectManager _connectManager;
        private readonly IUserManager _userManager;

        public ConnectService(IConnectManager connectManager, IUserManager userManager)
        {
            _connectManager = connectManager;
            _userManager = userManager;
        }

        public object Post(CreateConnectLink request)
        {
            return _connectManager.LinkUser(request.Id, request.ConnectUsername);
        }

        public object Post(CreateConnectInvite request)
        {
            return _connectManager.InviteUser(request.SendingUserId, request.ConnectUsername);
        }

        public void Delete(DeleteConnectLink request)
        {
            var task = _connectManager.RemoveConnect(request.Id);

            Task.WaitAll(task);
        }

        public async Task<object> Get(GetPendingGuests request)
        {
            var result = await _connectManager.GetPendingGuests().ConfigureAwait(false);

            return ToOptimizedResult(result);
        }

        public void Delete(DeleteAuthorization request)
        {
            var task = _connectManager.CancelAuthorization(request.Id);

            Task.WaitAll(task);
        }

        public object Get(GetLocalUser request)
        {
            var user = _userManager.Users
                .FirstOrDefault(i => string.Equals(i.ConnectUserId, request.ConnectUserId, StringComparison.OrdinalIgnoreCase));

            if (user == null)
            {
                throw new ResourceNotFoundException();
            }

            return ToOptimizedResult(new ConnectAuthenticationExchangeResult
            {
                AccessToken = user.ConnectAccessKey,
                LocalUserId = user.Id.ToString("N")
            });
        }
    }
}
