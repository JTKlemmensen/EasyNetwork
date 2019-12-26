using Moq;
using NUnit.Framework;
using EasyNetwork.Network;
using EasyNetwork.Network.Abstract;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyNetwork.Test
{
    public class TestSecureServerConnection
    {
        [Test]
        public void Test_EndHandshake()
        {
            var publicKey = new byte[] { 1, 2, 3 };
            var iv = new byte[] { 1, 2 };
            var encryptedIv = new byte[] { 2, 4 };
            var key = new byte[] { 5, 6 };
            var encryptedKey = new byte[] { 10, 12 };

            var mockAsymmetric = new Mock<IAsymmetricCipher>();
            mockAsymmetric.SetupSet(m => m.PublicKey = publicKey);
            mockAsymmetric.Setup(m => m.Encrypt(iv)).Returns(encryptedIv);
            mockAsymmetric.Setup(m => m.Encrypt(key)).Returns(encryptedKey);

            var mockSymmetric = new Mock<ISymmetricCipher>();
            mockSymmetric.SetupGet(m => m.IV).Returns(iv);
            mockSymmetric.SetupGet(m => m.Key).Returns(key);

            var mockConnection = new Mock<IConnection>();

            var secureClient = new SecureServerConnection(mockConnection.Object, mockAsymmetric.Object, mockSymmetric.Object);

            // Act
            mockConnection.Raise(i => i.OnDataReceived += null, publicKey);

            mockAsymmetric.VerifySet(m => m.PublicKey = publicKey);
            mockSymmetric.VerifyGet(m => m.IV);
            mockSymmetric.VerifyGet(m => m.Key);
            mockAsymmetric.Verify(m => m.Encrypt(iv));
            mockAsymmetric.Verify(m => m.Encrypt(key));
            mockConnection.Verify(m => m.SendData(encryptedIv));
            mockConnection.Verify(m => m.SendData(encryptedKey));
        }
        
        [Test]
        public void Test_ReceiveDataAfterHandshake()
        {
            var data = new byte[] { 2, 4, 6 };
            var encryptedData = new byte[] { 4, 6, 8 };
            var mockAsymmetric = new Mock<IAsymmetricCipher>();

            var mockSymmetric = new Mock<ISymmetricCipher>();
            mockSymmetric.Setup(m => m.Decrypt(encryptedData)).Returns(data).Verifiable();

            var mockConnection = new Mock<IConnection>();

            var secureClient = new SecureServerConnection(mockConnection.Object, mockAsymmetric.Object, mockSymmetric.Object);

            mockConnection.Raise(i => i.OnDataReceived += null, new byte[] { });
            byte[] receivedData = null;
            secureClient.OnDataReceived += (data) => receivedData = data;

            //Act
            mockConnection.Raise(i => i.OnDataReceived += null, encryptedData);

            mockSymmetric.Verify(m => m.Decrypt(encryptedData));
            Assert.AreEqual(data, receivedData);
        }
    }
}