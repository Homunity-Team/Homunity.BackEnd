using Homunity_Buisness_Logic;
using Xunit;

namespace Homunity.Tests.Unit
{
    public class PasswordHasherTests
    {
        [Fact]
        public void Hash_SamePassword_ReturnsSameHash()
        {
            // Arrange
            string password = "Test@123";

            // Act
            string hash1 = PasswordHasher.Hash(password);
            string hash2 = PasswordHasher.Hash(password);

            // Assert
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void Hash_DifferentPasswords_ReturnDifferentHashes()
        {
            // Arrange
            string password1 = "Test@123";
            string password2 = "Test@456";

            // Act
            string hash1 = PasswordHasher.Hash(password1);
            string hash2 = PasswordHasher.Hash(password2);

            // Assert
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void Hash_DoesNotReturnPlainTextPassword()
        {
            // Arrange
            string password = "Test@123";

            // Act
            string hash = PasswordHasher.Hash(password);

            // Assert
            Assert.NotEqual(password, hash);
        }
    }
}