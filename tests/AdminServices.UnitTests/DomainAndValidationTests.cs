using AdminServices.Application.Auth.Commands.Register;
using AdminServices.Application.Common.Behaviors;
using AdminServices.Application.Notes.Commands.CreateNote;
using AdminServices.Domain.Entities;
using FluentAssertions;
using FluentValidation;
using Xunit;

namespace AdminServices.UnitTests;

public class DomainAndValidationTests
{
    [Fact]
    public void Session_is_active_until_revoked_or_expired()
    {
        var session = new UserSession(Guid.NewGuid(), "sid-1", DateTime.UtcNow.AddHours(1));
        session.IsActive(DateTime.UtcNow).Should().BeTrue();
        session.IsActive(DateTime.UtcNow.AddHours(2)).Should().BeFalse();

        session.Revoke();
        var revokedAt = session.RevokedAt;
        session.IsActive(DateTime.UtcNow).Should().BeFalse();
        session.Revoke();
        session.RevokedAt.Should().Be(revokedAt, "revoking twice keeps the first time");
    }

    [Fact]
    public void Lab_account_user_has_no_local_password_and_is_a_regular_user()
    {
        var user = User.FromSso("sub-1", "ana@lab.zuritaserver.com", "Ana");
        user.SsoSub.Should().Be("sub-1");
        user.PasswordHash.Should().BeEmpty();
        user.Role.Should().Be("User");
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Validation_behavior_stops_invalid_requests_before_the_handler()
    {
        var behavior = new ValidationBehavior<CreateNoteCommand, string>(new[] { new CreateNoteCommandValidator() });
        var called = false;

        var act = () => behavior.Handle(new CreateNoteCommand { Title = "", Content = "" }, () => { called = true; return Task.FromResult("ok"); }, default);

        (await act.Should().ThrowAsync<ValidationException>()).Which.Errors.Should().Contain(e => e.PropertyName == "Title");
        called.Should().BeFalse();

        var ok = await behavior.Handle(new CreateNoteCommand { Title = "t", Content = "c" }, () => Task.FromResult("ok"), default);
        ok.Should().Be("ok");
    }

    [Fact]
    public void Register_validator_requires_email_password_and_name()
    {
        var result = new RegisterCommandValidator().Validate(new RegisterCommand { Email = "mal", Password = "123", FullName = "" });
        result.Errors.Select(e => e.PropertyName).Should().Contain(new[] { "Email", "Password", "FullName" });
    }
}
