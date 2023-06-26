﻿using BigBlueApi.Application.DTOs;
using BigBlueApi.Application.Mappers;
using BigBlueApi.Domain;
using BigBlueApi.Domain.IRepository;
using BigBlueApi.Persistence.Repository;

namespace BigBlueApi.Application.Services
{
    public class SessionServiceImp
    {
        private readonly ISessionRepository _repository;
        private readonly IUnitOfWork _uow;

        public SessionServiceImp(SessionRepository repository,IUnitOfWork uow)
        {
            _repository = repository;
            _uow = uow;
        }


        public async ValueTask<string> CreateSession(Session sessionDto)
        {
            var sesssion = new Session(
                sessionDto.MeetingId,
                sessionDto.Recorded,
                sessionDto.Name,
                sessionDto.ModeratorPassword,
                sessionDto.AttendeePassword,
                sessionDto.StartDateTime,
                sessionDto.EndDateTime,
                sessionDto.LimitCapacity
                );
           await _repository.CreateSession( sesssion );
            await _uow.SaveChangesAsync();
            return sesssion.MeetingId;
        }
        public async ValueTask<Session> Find(string meetingID)
        {
            var Result = await _repository.Find( meetingID );
            return Result;
        }
        public async Task EditSession(string id, SessionEditDto session)
        {
            await _repository.EditSession(id,
                SessionMapper
                .Map(session));
        }
        public async Task StopRunnig(string id)
        {
            await _repository.StopRunnig(id);
        }
    }
}