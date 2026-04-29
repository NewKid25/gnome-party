export function getEnemyImage(type:string): string {
    switch (type) {
        case "Skeleton":
            return "/img/Skeleton.svg";
        case "Goblin Archer":
            return "/img/Goblin Archer.svg";
        case "Forest Sprite":
            return "/img/Forest Sprite.svg";
        case "Cave Bat":
            return "/img/Cave Bat.svg";
        case "Gnombie Brute":
            return "/img/Gnombie Brute.svg";
        case "Gnome Eater":
            return "/img/Gnome Eater.svg";
        case "Necrognomancer":
            return "/img/Necrognomancer.svg";
        default:
            console.log("Default triggered because it was", type);
            return "/img/Skeleton.svg";
    }
}