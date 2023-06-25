using BigBlueApi.Domain;
using BigBlueApi.Models;
using BigBlueApi.Persistence;
using BigBlueButtonAPI.Core;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BigBlueApi.Controllers;

[ApiController]
[Route("[controller]")]
public class MeetController : ControllerBase
{
    private readonly BigBlueContext _context;
    private readonly BigBlueButtonAPIClient _client;

    public MeetController(BigBlueButtonAPIClient client, BigBlueContext context) =>
        (_context, _client) = (context, client);

    [NonAction]
    private async Task<bool> IsBigBlueSettingsOkAsync()
    {
        try
        {
            var result = await _client.IsMeetingRunningAsync(
                new IsMeetingRunningRequest { meetingID = Guid.NewGuid().ToString() }
            );
            if (result.returncode == Returncode.FAILED)
                return false;
            return true;
        }
        catch
        {
            return false;
        }
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> GetMeetingInformations([FromQuery] string mettingId)
    {
        var result = await _client.IsMeetingRunningAsync(
            new IsMeetingRunningRequest { meetingID = Guid.NewGuid().ToString() }
        );
        return Ok(result);
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> CreateMeeting([FromBody] CreateMeetingRequestModel request)
    {
        var meetingCreateRequest = new CreateMeetingRequest
        {
            name = request.Name,
            meetingID = request.MeetingId,
            record = true
        };
        var result = await _client.CreateMeetingAsync(meetingCreateRequest);
        if (result.returncode == Returncode.FAILED)
            return BadRequest("A Problem Has Been Occurred in Creating Meet.");

        var meetingInfoRequest = new GetMeetingInfoRequest { meetingID = request.MeetingId };
        var resultInfo = await _client.GetMeetingInfoAsync(meetingInfoRequest);

        var session = new Session(
            resultInfo.meetingID,
            resultInfo.recording.Value ? true : false,
            resultInfo.meetingName,
            resultInfo.moderatorPW,
            resultInfo.attendeePW,
            DateTime.Now,
            resultInfo.endTime is null
                ? DateTime.Now.AddHours((double)resultInfo.endTime)
                : DateTime.Now
        );

        System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
        xmlDoc.LoadXml(result.ToString()!);
        string jsonResult = JsonConvert.SerializeXmlNode(xmlDoc, Formatting.Indented, true);

        return Ok(jsonResult);
    }

    private async Task<int> InsertSession(Session session)
    {
        await _context.AddAsync(session);
        return await _context.SaveChangesAsync();
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> JoinMeeting([FromBody] JoinMeetingRequestModel request)
    {
        var requestJoin = new JoinMeetingRequest { meetingID = request.MeetingId };
        if (request.Role == "1")
        {
            requestJoin.password = request.Password;
            requestJoin.userID = "10000";
            requestJoin.fullName = "Admin";
        }
        else
        {
            requestJoin.password = request.Password;
            requestJoin.userID = "20000";
            requestJoin.fullName = "User";
        }
        var url = _client.GetJoinMeetingUrl(requestJoin);
        return Redirect(url);
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> EndMeeting(string meetingId, string password)
    {
        var result = await _client.EndMeetingAsync(
            new EndMeetingRequest { meetingID = meetingId, password = password }
        );
        if (result.returncode == Returncode.FAILED)
            return BadRequest(result.message);
        return Ok("Meeting is End.");
    }
}
