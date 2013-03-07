using System;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Crypto.Parameters;
using System.IO;

namespace Bitcoin_Tool.Crypto
{
	public class ECKeyPair
	{
		ECDomainParameters ecParams = new ECDomainParameters(
			SecNamedCurves.GetByName("secp256k1").Curve, SecNamedCurves.GetByName("secp256k1").G, SecNamedCurves.GetByName("secp256k1").N);
		public Byte[] privKey { get; private set; }
		public Byte[] pubKey { get; private set; }
		public Boolean isCompressed { get; private set; }

		public ECKeyPair(Byte[] privKey, Byte[] pubKey = null, Boolean compressed = false)
		{
			this.privKey = privKey;
			if (pubKey != null)
			{
				this.pubKey = pubKey;
				this.isCompressed = pubKey.Length <= 33;
			}
			else
			{
				calcPubKey(compressed);
			}
		}

		public void compress(bool comp)
		{
			if (isCompressed == comp) return;
			ECPoint point = ecParams.Curve.DecodePoint(pubKey);
			if (comp)
				pubKey = compressPoint(point).GetEncoded();
			else
				pubKey = decompressPoint(point).GetEncoded();
			isCompressed = comp;
		}

		public Boolean verifySignature(Byte[] data, Byte[] sig)
		{
			ECDsaSigner signer = new ECDsaSigner();
			signer.Init(false, new ECPublicKeyParameters(ecParams.Curve.DecodePoint(pubKey), ecParams));
			using (Asn1InputStream asn1stream = new Asn1InputStream(sig))
			{
				Asn1Sequence seq = (Asn1Sequence)asn1stream.ReadObject();
				return signer.VerifySignature(data, ((DerInteger)seq[0]).PositiveValue, ((DerInteger)seq[1]).PositiveValue);
			}
		}

		public Byte[] signData(Byte[] data)
		{
			if (privKey == null)
				throw new InvalidOperationException();
			ECDsaSigner signer = new ECDsaSigner();
			signer.Init(true, new ECPrivateKeyParameters(new BigInteger(1, privKey), ecParams));
			BigInteger[] sig = signer.GenerateSignature(data);
			using (MemoryStream ms = new MemoryStream())
			using (Asn1OutputStream asn1stream = new Asn1OutputStream(ms))
			{
				DerSequenceGenerator seq = new DerSequenceGenerator(asn1stream);
				seq.AddObject(new DerInteger(sig[0]));
				seq.AddObject(new DerInteger(sig[1]));
				seq.Close();
				return ms.ToArray();
			}
		}
		private void calcPubKey(bool comp) {

			ECPoint point = ecParams.G.Multiply(new BigInteger(1, privKey));
			this.pubKey = point.GetEncoded();
			compress(comp);
		}

		private ECPoint compressPoint(ECPoint point)
		{
			return new FpPoint(ecParams.Curve, point.X, point.Y, true);
		}

		private ECPoint decompressPoint(ECPoint point)
		{
			return new FpPoint(ecParams.Curve, point.X, point.Y, false);
		}
	}
}
