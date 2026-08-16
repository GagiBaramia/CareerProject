namespace CareerProject.Shared.Entities;

// Fixed GUIDs so the seed migration is reproducible across environments.
public static class SkillSeedData
{
    public static readonly Skill[] Skills =
    [
        new() { Id = Guid.Parse("6a908eef-9fa3-4c1c-a091-0a351fd29f7f"), Name = "C#" },
        new() { Id = Guid.Parse("f6e3e75e-f4c7-4445-a950-bdcd242a2766"), Name = ".NET" },
        new() { Id = Guid.Parse("7151df15-375b-4fd8-bf53-ec4a99872f10"), Name = "Java" },
        new() { Id = Guid.Parse("858b1dff-f15b-44fc-8335-849dd4c1e498"), Name = "Spring Boot" },
        new() { Id = Guid.Parse("55f9afbb-f68b-4709-a189-8cffe25cec68"), Name = "JavaScript" },
        new() { Id = Guid.Parse("e8f70325-47ee-4e3b-8007-c25c7395daad"), Name = "TypeScript" },
        new() { Id = Guid.Parse("044ba8a0-51eb-41ed-a4a7-63d30b4835d3"), Name = "Angular" },
        new() { Id = Guid.Parse("faad0c20-7a75-4d57-8317-821b8d88cbb5"), Name = "React" },
        new() { Id = Guid.Parse("ee3eedd0-25c4-4633-9154-c8d34c9825ec"), Name = "Node.js" },
        new() { Id = Guid.Parse("645360b7-3a40-4b6e-b2af-7c63ba8ab959"), Name = "HTML" },
        new() { Id = Guid.Parse("afff14fb-4893-49c0-9e13-c8b5a2fb97dc"), Name = "CSS" },
        new() { Id = Guid.Parse("2b87718d-b638-48d5-a8f5-0c2566974803"), Name = "PostgreSQL" },
        new() { Id = Guid.Parse("04bef3cb-a298-49b1-ae40-7921b7530b1b"), Name = "SQL" },
        new() { Id = Guid.Parse("7110b157-31c4-4683-9f44-909734f94196"), Name = "Redis" },
        new() { Id = Guid.Parse("a205340a-6964-42e9-9322-7a52dd454811"), Name = "RabbitMQ" },
        new() { Id = Guid.Parse("776a6a2c-c04b-4a1c-a294-439df0eadeed"), Name = "Docker" },
        new() { Id = Guid.Parse("8a22c113-106f-42af-b5c4-7a23fe1ec239"), Name = "Git" },
        new() { Id = Guid.Parse("af530ac8-9a3d-4628-ac00-f4262e35f226"), Name = "REST API" },
        new() { Id = Guid.Parse("984edcb0-ef2a-4ede-9c31-e1d7008e8513"), Name = "AWS" },
    ];
}
