using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutOfSchool.Common;

namespace OutOfSchool.Services.Models.Configurations.Base;

public abstract class BusinessEntityWithContactsConfiguration<TBase> : BusinessEntityConfiguration<TBase>
    where TBase : BusinessEntity, IHasContacts
{
    public override void Configure(EntityTypeBuilder<TBase> builder)
    {
        base.Configure(builder);
        builder.OwnsMany(e => e.Contacts, contacts =>
        {
            contacts.WithOwner().HasForeignKey("OwnerId");

            contacts.Property<long>("Id");
            contacts.HasKey("Id");
            
            contacts.Property(c => c.Title)
                .IsRequired()
                .HasMaxLength(Constants.ContactsTitleMaxLength);

            // Address
            contacts.OwnsOne(c => c.Address, a =>
            {
                a.Property(p => p.Street)
                    .IsRequired()
                    .HasMaxLength(60);

                a.Property(p => p.BuildingNumber)
                    .IsRequired()
                    .HasMaxLength(15);

                a.Property(p => p.CATOTTGId)
                    .IsRequired();
            });

            // Phones
            contacts.OwnsMany(c => c.Phones, p =>
            {
                p.WithOwner().HasForeignKey("ContactsId");

                p.Property<long>("Id");
                p.HasKey("Id");

                p.Property(pr => pr.Type)
                    .HasMaxLength(Constants.ContactsTitleMaxLength);

                p.Property(pr => pr.Number)
                    .IsRequired()
                    .HasMaxLength(Constants.MaxPhoneNumberLengthWithPlusSign);

                p.HasIndex("Number"); // Additional index for search
            });

            // Emails
            contacts.OwnsMany(c => c.Emails, e =>
            {
                e.WithOwner().HasForeignKey("ContactsId");

                e.Property<long>("Id");
                e.HasKey("Id");

                e.Property(p => p.Type).HasMaxLength(Constants.MaxEmailTypeLength);
                e.Property(p => p.Address).HasMaxLength(Constants.MaxEmailAddressLength);

                e.HasIndex("Address"); // Additional index for search
            });

            // SocialNetworks
            contacts.OwnsMany(c => c.SocialNetworks, s =>
            {
                s.WithOwner().HasForeignKey("ContactsId");

                s.Property<long>("Id");
                s.HasKey("Id");

                s.Property(p => p.Url).HasMaxLength(Constants.MaxUnifiedUrlLength);
            });
        });
    }
}