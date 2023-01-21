using System;
using System.IO;

using CSharpDatabase.Common;

namespace CSharpDatabase.Application
{
  // This class serializes a BankAccountModel into byte[] for using with RecordStorage;
  public class BankAccountsSerializer
  {
    public byte[] Serialize(BankAccountModel account)
    {
      byte[] accNumberBytes = System.Text.Encoding.UTF8.GetBytes(account.AccountNumber);
      byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(account.FirstName);
      byte[] lastNameBytes = System.Text.Encoding.UTF8.GetBytes(account.LastName);
      byte[] peselBytes = System.Text.Encoding.UTF8.GetBytes(account.Pesel);


      // Preparing byte container for account data
      var accountData = new byte[
          16 +                    // 16 bytes for Id (Guid)
          4 +                     // 4 bytes indicate the length of Breed (string converted to byte[])
          accNumberBytes.Length + // bytes for enxrypted account number string
          4 +                     // 4 bytes indicate the length of the Name (string converted to byte[])
          nameBytes.Length +      // bytes for first name 
          4 +                     // 4 bytes indicate the length of the SecondName (string converted to byte[])
          lastNameBytes.Length +  // bytes for last name 
          4 +                     // 4 bytes for Age (int)
          4 +                     // 4 bytes indicate the length of the Pesel (string converted to byte[])
          peselBytes.Length +     // bytes for Pesel
          8                       // 8 bytes for Balance (ulong)
      ];

      // Inserting account data into container:

      int position = 0; // Position in buffer

      //	Id (fixed size)
      Buffer.BlockCopy(
                src: account.Id.ToByteArray(),
          srcOffset: 0,
                dst: accountData,
          dstOffset: position,
              count: 16
      );
      position += 16;


      //	Encrypted account number (length of Account number + Account number itself)
      Buffer.BlockCopy(
                src: ByteConverter.ToBytes((int)accNumberBytes.Length),
          srcOffset: 0,
                dst: accountData,
          dstOffset: position,
              count: 4
      );
      position += 4;

      Buffer.BlockCopy(
                src: accNumberBytes,
          srcOffset: 0,
                dst: accountData,
          dstOffset: position,
              count: accNumberBytes.Length
      );
      position += accNumberBytes.Length;


      //	FirstName (length of Name + Name itself)
      Buffer.BlockCopy(
                src: ByteConverter.ToBytes((int)nameBytes.Length),
          srcOffset: 0,
                dst: accountData,
          dstOffset: position,
              count: 4
      );
      position += 4;

      Buffer.BlockCopy(
                src: nameBytes,
          srcOffset: 0,
                dst: accountData,
          dstOffset: position,
              count: nameBytes.Length
      );
      position += nameBytes.Length;


      //	LastName (length of LastName + LastName itself)
      Buffer.BlockCopy(
                src: ByteConverter.ToBytes((int)lastNameBytes.Length),
          srcOffset: 0,
                dst: accountData,
          dstOffset: position,
              count: 4
      );
      position += 4;

      Buffer.BlockCopy(
                src: lastNameBytes,
          srcOffset: 0,
                dst: accountData,
          dstOffset: position,
              count: lastNameBytes.Length
      );
      position += lastNameBytes.Length;


      //	Age (fixed size)
      Buffer.BlockCopy(
                src: ByteConverter.ToBytes((int)account.Age),
          srcOffset: 0,
                dst: accountData,
          dstOffset: position,
              count: 4
      );
      position += 4;


      //	Pesel (length of Pesel + Pesel itself)
      Buffer.BlockCopy(
                src: ByteConverter.ToBytes((int)peselBytes.Length),
          srcOffset: 0,
                dst: accountData,
          dstOffset: position,
              count: 4
      );
      position += 4;

      Buffer.BlockCopy(
                src: peselBytes,
          srcOffset: 0,
                dst: accountData,
          dstOffset: position,
              count: peselBytes.Length
      );
      position += peselBytes.Length;


      //	Balance (fixed size)
      Buffer.BlockCopy(
                src: ByteConverter.ToBytes((ulong)account.Balance),
          srcOffset: 0,
                dst: accountData,
          dstOffset: position,
              count: 4
      );
      position += 4;


      return accountData;
    }

    public BankAccountModel Deserializer(byte[] data)
    {
      int position = 0; // Position in buffer

      // Read id
      Guid id = BufferUtils.ReadBufferGuid(data, (uint)position);
      position += 16;


      // Read account number
      var accNumberLength = BufferUtils.ReadBufferInt32(data, (uint)position);
      position += 4;

      if (accNumberLength < 0 || accNumberLength > (16 * 1024))
        throw new Exception("Invalid string length: " + accNumberLength);

      string accountNumber = System.Text.Encoding.UTF8.GetString(data, position, accNumberLength);
      position += accNumberLength;


      // Read name
      var nameLength = BufferUtils.ReadBufferInt32(data, (uint)position);
      position += 4;

      if (nameLength < 0 || nameLength > (16 * 1024))
        throw new Exception("Invalid string length: " + nameLength);

      string firstName = System.Text.Encoding.UTF8.GetString(data, position, nameLength);
      position += nameLength;


      // Read last name
      var lastNameLength = BufferUtils.ReadBufferInt32(data, (uint)position);
      position += 4;

      if (lastNameLength < 0 || lastNameLength > (16 * 1024))
        throw new Exception("Invalid string length: " + lastNameLength);

      string lastName = System.Text.Encoding.UTF8.GetString(data, position, lastNameLength);
      position += lastNameLength;


      // Read age
      int age = BufferUtils.ReadBufferInt32(data, (uint)position);
      position += 4;


      // Read pesel
      var peselLength = BufferUtils.ReadBufferInt32(data, (uint)position);
      position += 4;

      if (peselLength < 0 || peselLength > (16 * 1024))
        throw new Exception("Invalid string length: " + lastNameLength);

      string pesel = System.Text.Encoding.UTF8.GetString(data, position, peselLength);
      position += peselLength;


      // Read balance
      ulong balance = BufferUtils.ReadBufferUInt64(data, (uint)position);
      position += 8;

      // Return constructed model
      return new BankAccountModel
      (
        id: id,
        accountNumber: accountNumber,
        firstName: firstName,
        lastName: lastName,
        age: age,
        pesel: pesel,
        balance: balance
      );
    }
  }
}
