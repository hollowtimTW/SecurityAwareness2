using FluentAssertions;
using Xunit;
using SecurityAwareness.Application.Services;

namespace SecurityAwareness.Tests.Unit;

/// <summary>
/// Tests for BCrypt password hashing wrapper.
/// </summary>
public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Hash_ValidPassword_ProducesDifferentHashEachTime()
    {
        var hash1 = _hasher.Hash("Admin@123");
        var hash2 = _hasher.Hash("Admin@123");

        hash1.Should().NotBeNullOrEmpty();
        hash2.Should().NotBeNullOrEmpty();
        hash1.Should().NotBe(hash2, "BCrypt salt makes every hash unique");
    }

    [Fact]
    public void Hash_StartsWithBcryptPrefix()
    {
        var hash = _hasher.Hash("Test123!");
        hash.Should().StartWith("$2");
    }

    [Fact]
    public void Verify_CorrectPassword_ReturnsTrue()
    {
        var hash = _hasher.Hash("MySecurePass!");
        _hasher.Verify("MySecurePass!", hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        var hash = _hasher.Hash("CorrectPass");
        _hasher.Verify("WrongPass", hash).Should().BeFalse();
    }

    [Fact]
    public void Verify_EmptyInput_ReturnsFalse()
    {
        var hash = _hasher.Hash("Valid");
        _hasher.Verify("", hash).Should().BeFalse();
        _hasher.Verify("Valid", "").Should().BeFalse();
    }

    [Fact]
    public void Hash_EmptyPassword_Throws()
    {
        Action act = () => _hasher.Hash("");
        act.Should().Throw<ArgumentException>();
    }
}
