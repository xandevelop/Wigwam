test | As a user, I can give bob a nice message
	print a nice message for bob

test | As a horrible user, I can give bob a horrible message
	print a horrible message for bob

func | print a nice message for bob
	say hello | bob

func | print a horrible message for bob
	say go away | bob

func | say hello | username
	echo | hello, ${username}

func | say go away | username
	echo | go away, ${username}

