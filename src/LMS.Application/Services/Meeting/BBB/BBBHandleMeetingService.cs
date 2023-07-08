﻿using LIMS.Application.Services.Database.BBB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;
using BigBlueApi.Application.DTOs;
using BigBlueButtonAPI.Core;
using LIMS.Application.Models;
using LIMS.Domain.Entity;
using LIMS.Application.Models.Http.BBB;

namespace LIMS.Application.Services.Meeting.BBB
{
    public class BBBHandleMeetingService
    {
        private readonly BigBlueButtonAPIClient _client;
        private readonly BBBMeetingServiceImpl _meetingService;
        private readonly BBBServerServiceImpl _serverService;
        private readonly BBBUserServiceImpl _userService;
        private readonly BBBMemberShipServiceImpl _memberShipService;

        public BBBHandleMeetingService(
            BigBlueButtonAPIClient client,
            BBBMeetingServiceImpl sessionService,
            BBBServerServiceImpl serverService,
            BBBUserServiceImpl userService,
            BBBMemberShipServiceImpl memberShipService
        ) =>
            (_client, _meetingService, _serverService, _userService, _memberShipService) = (
                client,
                sessionService,
                serverService,
                userService,
                memberShipService
            );

        public async Task<SingleResponse<ServerAddEditDto>> UseCapableServerCreateMeeting()
        {
            var server = await _serverService.MostCapableServer();

            if (!server.Success)
                if (server.Exception is not null)
                    return SingleResponse<ServerAddEditDto>.OnFailed(server.Exception.Data.ToString());
                else
                    return SingleResponse<ServerAddEditDto>.OnFailed(server.OnFailedMessage);
            else
               return SingleResponse<ServerAddEditDto>.OnSuccess(server.Result);
        }

        public async ValueTask<SingleResponse<string>> HandleCreateMeeting(MeetingAddEditDto meeting)
        {
            var createMeeting = await _meetingService.CreateNewMeetingAsync(meeting);

            if (!createMeeting.Success)
                if (createMeeting.Exception is not null)
                    return SingleResponse<string>.OnFailed(createMeeting.Exception.Data.ToString());
                else
                    return SingleResponse<string>.OnFailed(createMeeting.OnFailedMessage);
            else
                return SingleResponse<string>.OnSuccess(createMeeting.Result);
        }

        public async ValueTask<SingleResponse<bool>> IsBigBlueButtonOk(string meetingId)
        {
            try
            {
                var result = await _client.IsMeetingRunningAsync(
                    new IsMeetingRunningRequest
                        { meetingID = meetingId }
                );

                if (result.returncode == Returncode.FAILED)
                    return SingleResponse<bool>.OnFailed(result.message);
                else
                    return SingleResponse<bool>.OnSuccess(true);
            }
            catch (Exception exception)
            {
                return SingleResponse<bool>.OnFailed(exception.Data.ToString());
            }
        }

        //public async ValueTask<SingleResponse<bool>> CanJoinOnMeetingHandler(JoinMeetingRequestModel joinRequest)
        //{
        //    var server = await _meetingService
        //        .FindMeetingWithMeetingId(joinRequest.MeetingId);
        //    if (!server.Success)
        //        if (server.Exception is not null)
        //            return SingleResponse<bool>.OnFailed(server.Exception.Data.ToString());
        //        else
        //            return SingleResponse<bool>.OnFailed(server.OnFailedMessage);

        //    var canJoinOnMeeting = await _memberShipService.CanJoinUserOnMeetingAsync(id);
        //    if (!canJoinOnMeeting.Success)
        //        if (server.Exception is not null)
        //            return StatusCode(500);
        //        else
        //            return BadRequest(server.OnFailedMessage);
        //    if (!canJoinOnMeeting.Result)
        //        return BadRequest("Joining into This Class Not Accessed.");

        //    var canJoinOnServer = await _serverService.CanJoinServer(server.Result.Id);
        //    if (!canJoinOnServer.Success)
        //        if (canJoinOnServer.Exception is not null)
        //            return StatusCode(500);
        //        else
        //            return BadRequest(canJoinOnServer.OnFailedMessage);
        //    if (!canJoinOnServer.Result)
        //        return BadRequest("Server is Full.");

        //    var cnaLoginOnMeeting = _meetingService.CanLoginOnExistMeeting(joinRequest.MeetingId, joinRequest.UserInformations.Role, joinRequest.MeetingPassword).Result;
        //    if (!cnaLoginOnMeeting.Success)
        //        if (cnaLoginOnMeeting.Exception is not null)
        //            return StatusCode(500);
        //        else
        //            return BadRequest(cnaLoginOnMeeting.OnFailedMessage);
        //    if (!cnaLoginOnMeeting.Result)
        //        return BadRequest("Cannot Join Into Meeting.");
        //}
    }

   
}
